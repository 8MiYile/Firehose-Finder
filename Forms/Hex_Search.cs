﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FirehoseFinder
{
    public partial class Hex_Search : Form
    {
        Func func = new Func();
        List<File_Struct> orig_list = new List<File_Struct>(); //Основной список (полный путь к файлу - хеш)
        List<File_Struct> dubl_list = new List<File_Struct>(); //Дубликаты (полный путь к файлу - хеш)
        Hashtable TableGroups = new Hashtable();
        readonly ResourceManager LocRes = new ResourceManager("FirehoseFinder.Properties.Resources", typeof(Formfhf).Assembly);

        public Hex_Search()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Начинаем со вкладки "Поиск по маске"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hex_Search_Shown(object sender, EventArgs e)
        {
            tabControl1.SelectedTab = tabPage_mask;
        }
        #region Вкладка "Поиск по маске
        private void TextBox_byte_search_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_byte_search.Text)) button_start_search.Enabled = false;
            else
            {
                textBox_byte_search.Text = func.DelUnknownChars(textBox_byte_search.Text, Func.System_Count.hex);
                textBox_byte_search.SelectionStart = textBox_byte_search.TextLength;
                button_start_search.Enabled = true;
            }
        }

        private void TextBox_hexsearch_TextChanged(object sender, EventArgs e)
        {
            textBox_byte_search.Text = string.Empty;
            byte[] convstrtobyte = new byte[textBox_hexsearch.Text.Count()];
            for (int i = 0; i < convstrtobyte.Length; i++)
            {
                convstrtobyte[i] = (byte)textBox_hexsearch.Text[i];
                textBox_byte_search.Text += string.Format("{0:X2}", convstrtobyte[i]);
            }
        }

        private void ListView_search_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(listView_search.SelectedItems[0].Text);
            toolStripStatusLabel_search.Text = LocRes.GetString("hex_note_copyoffset");
        }

        private void RadioButton_search_text_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_search_text.Checked == true)
            {
                textBox_byte_search.Enabled = false;
                textBox_hexsearch.Enabled = true;
                textBox_hexsearch.Text = string.Empty;
            }
        }

        private void RadioButton_byte_search_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_byte_search.Checked == true)
            {
                textBox_byte_search.Enabled = true;
                textBox_byte_search.Text = string.Empty;
                textBox_hexsearch.Enabled = false;
            }
        }

        private void Button_start_search_Click(object sender, EventArgs e)
        {
            Dictionary<string, long> filestosearch = new Dictionary<string, long>();
            if (backgroundWorker_hex_search.IsBusy)
            {
                //Если паралельный поток ещё выполняется, предлагаем остановить
                if (MessageBox.Show(LocRes.GetString("hex_mess_stopoper"),
                    LocRes.GetString("hex_warn_stopoper"),
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2,
                    MessageBoxOptions.DefaultDesktopOnly) == DialogResult.OK)
                {

                    backgroundWorker_hex_search.CancelAsync();
                    toolStripStatusLabel_search.Text = LocRes.GetString("hex_note_cancelsearch");
                    toolStripProgressBar_search.Value = 0;
                }
            }
            listView_search.Items.Clear(); //Очищаем элементы
            listView_search.Groups.Clear(); //Очищаем группы элементов
            TableGroups.Clear();//Очищаем таблицу групп
            toolStripStatusLabel_search.Text = string.Empty;
            if (textBox_byte_search.Text.Length % 2 != 0) textBox_byte_search.Text = "0" + textBox_byte_search.Text;
            if (radioButton_file.Checked)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(openFileDialog1.FileName);
                    button_start_search.Text = fi.FullName;
                    filestosearch.Add(fi.FullName, fi.Length);
                }
            }
            if (radioButton_dir.Checked)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    button_start_search.Text = folderBrowserDialog1.SelectedPath;
                    filestosearch = func.WFiles(folderBrowserDialog1.SelectedPath, false);
                }
            }
            if (filestosearch.Count > 0)
            {
                List<string> inputsearch = new List<string>();
                foreach (KeyValuePair<string, long> filename in filestosearch)
                {
                    FileInfo fileInfo = new FileInfo(filename.Key);
                    inputsearch.Add(filename.Key);
                    //Создаём группы по именам файлов
                    CreateTableGroups(fileInfo.Name);
                }
                //Создаём сущность Словарь+поиск и её отправляем в поток
                toolStripStatusLabel_search.Text = LocRes.GetString("hex_note_process");
                backgroundWorker_hex_search.RunWorkerAsync(new Search_Hex(inputsearch, textBox_byte_search.Text));
            }
        }

        private void BackgroundWorker_hex_search_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Search_Hex hex_search = (Search_Hex)e.Argument;
            int countfile = 1; //Счётчик обработанных файлов
            int currfiles = hex_search.FullFileNames.Count; //Всего файлов для обработки
            //Перебираем всю коллекцию файлов для поиска по очереди
            foreach (string fullfilename in hex_search.FullFileNames)
            {
                FileInfo fi = new FileInfo(fullfilename);
                List<Search_Result> addr_value_file = new List<Search_Result>(); //Результат выполнения потока (адрес, строка поиска, имя файла)
                int maxbytes = 268435456;//536870912;
                if (fi.Length >= hex_search.SearchString.Length / 2) //Длина файла не менее длины строки поиска, иначе вываливаемся
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(fullfilename, FileMode.Open)))
                    {
                        //Перегнали файл в отсортированный список массивов байт
                        if (fi.Length <= maxbytes) maxbytes = Convert.ToInt32(fi.Length);
                        int countarray = Convert.ToInt32(fi.Length / maxbytes); //Размер двумерного массива для больших файлов
                        byte[] chunk = new byte[maxbytes];
                        SortedList<int, byte[]> chunkarray = new SortedList<int, byte[]>();
                        for (int i = 0; i < countarray; i++)
                        {
                            chunk = reader.ReadBytes(maxbytes);
                            chunkarray[i] = chunk;
                            if (countarray!=0&&hex_search.FullFileNames.Count==1) worker.ReportProgress(i*100/countarray);
                        }
                        //Ищем совпадения и фиксируем адрес и значение
                        addr_value_file.AddRange(CompareBytes(maxbytes, chunkarray, fi.Name));
                    }
                }
                //Завершающие процедуры для одного файла из списка
                worker.ReportProgress(countfile * 100 / currfiles, addr_value_file);
                countfile++;
            }
        }

        private void BackgroundWorker_hex_search_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) toolStripStatusLabel_search.Text = e.Error.Message;
            else
            {
                toolStripProgressBar_search.Value = 0;
                if (listView_search.Items.Count==0) toolStripStatusLabel_search.Text = LocRes.GetString("hex_matches") + '\u0020' +
                        LocRes.GetString("hex_not") + '\u0020' +
                        LocRes.GetString("hex_found");
                else toolStripStatusLabel_search.Text = LocRes.GetString("hex_found") + '\u0020' +
                        listView_search.Items.Count.ToString() + '\u0020' +
                        LocRes.GetString("hex_matches") + '\u0020' +
                        LocRes.GetString("hex_in") + '\u0020' +
                        listView_search.Groups.Count.ToString() + '\u0020' +
                        LocRes.GetString("hex_from") + '\u0020' +
                        TableGroups.Count.ToString() + '\u0020' +
                        LocRes.GetString("hex_files");
            }
        }

        private void BackgroundWorker_hex_search_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState!=null)
            {
                List<Search_Result> result = (List<Search_Result>)e.UserState;
                //Заполняем листвью списком совпадений
                if (result.Count == 1) toolStripStatusLabel_search.Text = LocRes.GetString("hex_matches") + '\u0020' +
                        LocRes.GetString("hex_in") + '\u0020' +
                        LocRes.GetString("file") + '\u0020' +
                        result[0].File_Name + '\u0020' +
                        LocRes.GetString("hex_not") + '\u0020' +
                        LocRes.GetString("hex_found") + '\u0020' + '\u002D' + '\u0020' +
                        e.ProgressPercentage.ToString() + '\u0025' + '\u0020' +
                        LocRes.GetString("hex_processed");
                else
                {
                    foreach (Search_Result sr in result)
                    {
                        if (!string.IsNullOrEmpty(sr.Adress_hex))
                        {
                            //Проверяем наличие группы
                            if (!listView_search.Groups.Contains((ListViewGroup)TableGroups[sr.File_Name]))
                            {
                                listView_search.Groups.Add((ListViewGroup)TableGroups[sr.File_Name]);
                            }
                            ListViewItem hsearchres = new ListViewItem("0x" + sr.Adress_hex.ToUpper());
                            hsearchres.SubItems.Add(sr.Result_String);
                            hsearchres.SubItems.Add(sr.File_Name);
                            hsearchres.Group=(ListViewGroup)TableGroups[sr.File_Name];
                            listView_search.Items.Add(hsearchres);
                            toolStripStatusLabel_search.Text = LocRes.GetString("hex_processed") + '\u0020' +
                                LocRes.GetString("file") + '\u0020' +
                                sr.File_Name + '\u002C' + '\u0020' +
                                LocRes.GetString("hex_found") + '\u0020' +
                                listView_search.Items.Count.ToString() + '\u0020' +
                                LocRes.GetString("hex_matches");
                        }
                    }
                    for (int i = 0; i < listView_search.Columns.Count; i++) listView_search.Columns[i].Width = -1;
                }
            }
            toolStripProgressBar_search.Value = e.ProgressPercentage;
        }

        private List<Search_Result> CompareBytes(int maxbytes, SortedList<int, byte[]> dumpbytes, string filename)
        {
            List<Search_Result> search_Results = new List<Search_Result>
            {
                new Search_Result(string.Empty, string.Empty, filename)
            };
            byte[] searchstringinbytes = new byte[textBox_byte_search.Text.Length / 2];
            int comparecount = 0; //Счётчик для положения байт строки поиска
            for (int i = 0; i < textBox_byte_search.Text.Length / 2; i++)
            {
                searchstringinbytes[i] = Convert.ToByte(textBox_byte_search.Text.Substring(i * 2, 2), 16);
            }
            //Цикл для совпадений
            foreach (KeyValuePair<int, byte[]> item in dumpbytes)
            {
                //Выполняется для одиночного массива из списка массивов
                for (int i = 0; i < item.Value.Count(); i++)
                {
                    if (item.Value[i].Equals(searchstringinbytes[comparecount]))
                    {
                        comparecount++;
                        if (comparecount >= searchstringinbytes.Length)
                        {
                            //Получили совпадение
                            int frontdump = Convert.ToInt32(textBox_addfirst.Text); //Количество байт перед строкой поиска
                            int reardump = Convert.ToInt32(textBox_addlast.Text); //Количество байт после строки поиска
                            int addr = i - (searchstringinbytes.Length - 1);
                            Search_Result sr = new Search_Result(Convert.ToString((item.Key * maxbytes)+addr, 16), string.Empty, filename);
                            //Меняем количество символов для чтения пред и после поиска
                            if (addr - frontdump < 0) frontdump = addr;
                            if (addr + searchstringinbytes.Length + reardump > item.Value.Count()) reardump = Convert.ToInt32(item.Value.Count() - (addr + searchstringinbytes.Length));
                            int reslen = frontdump + searchstringinbytes.Length + reardump;
                            byte[] bytetores = new byte[reslen];
                            for (int br = 0; br < reslen; br++)
                            {
                                bytetores[br] = item.Value[addr-frontdump+br];
                            }
                            sr.Result_String = func.ByEight(bytetores, reslen);//BitConverter.ToString(item.Value, addr-frontdump, reslen);
                            //Сбросили счётчик совпадений
                            comparecount = 0;
                            search_Results.Add(sr);
                        }
                    }
                    else comparecount = 0;
                }
            }
            return search_Results;
        }
        /// <summary>
        /// Создаём таблицу групп
        /// </summary>
        /// <param name="subitemstr"></param>
        /// <returns></returns>
        private Hashtable CreateTableGroups(string subitemstr)
        {
            Hashtable groups = new Hashtable();
            if (!TableGroups.ContainsKey(subitemstr))
            {
                ListViewGroup group = new ListViewGroup(subitemstr, HorizontalAlignment.Left);
                TableGroups.Add(subitemstr, group);
            }
            return groups;
        }
        #endregion
        #region Вкладка "Различия между файлами"
        #endregion
        #region Вкладка "Дубликаты файлов"
        private byte[] HashFile(string filename)
        {
            byte[] hashfile = null;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    try
                    {
                        fileStream.Position = 0;
                        hashfile = mySHA256.ComputeHash(fileStream);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            return hashfile;
        }
        private void CheckDubs_SinglePath(List<File_Struct> worklist)
        {
            listView_dubl_files.Items.Clear(); //Очистили элементы листвью
            listView_dubl_files.Groups.Clear(); //Очистили группы листвью
            TableGroups.Clear(); //Очистили таблицу групп
            if (worklist.Count > 1)
            {
                for (int i = 0; i < worklist.Count - 1; i++)
                {
                    for (int k = i + 1; k < worklist.Count; k++)
                    {
                        if (worklist[i].HashCodeFile.Equals(worklist[k].HashCodeFile) && !worklist[k].Dubl) //Найден дубликат
                        {
                            CreateTableGroups(worklist[i].FullFileName); //Создаём группу, если не была раньше создана
                            //Проверяем на наличие группы в листвью
                            if (!listView_dubl_files.Groups.Contains((ListViewGroup)TableGroups[worklist[i].FullFileName]))
                                listView_dubl_files.Groups.Add((ListViewGroup)TableGroups[worklist[i].FullFileName]);
                            worklist[k].Dubl = true;
                            ListViewItem dublfile = new ListViewItem(worklist[k].FullFileName);
                            dublfile.SubItems.Add(worklist[k].FiLen.ToString("### ### ### ##0"));
                            dublfile.Group = (ListViewGroup)TableGroups[worklist[i].FullFileName];
                            listView_dubl_files.Items.Add(dublfile);
                            listView_dubl_files.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                        }
                    }
                }
            }
        }
        private void CheckDubs()
        {
            listView_dubl_files.Items.Clear(); //Очистили элементы листвью
            listView_dubl_files.Groups.Clear(); //Очистили группы листвью
            TableGroups.Clear(); //Очистили таблицу групп
            if (orig_list.Count > 0 && dubl_list.Count > 0)
            {
                for (int i = 0; i < orig_list.Count; i++)
                {
                    for (int k = 0; k < dubl_list.Count; k++)
                    {
                        if (orig_list[i].HashCodeFile.Equals(dubl_list[k].HashCodeFile) && !dubl_list[k].Dubl) //Найден дубликат
                        {
                            CreateTableGroups(orig_list[i].FullFileName); //Создаём группу, если не была раньше создана
                            //Проверяем на наличие группы в листвью
                            if (!listView_dubl_files.Groups.Contains((ListViewGroup)TableGroups[orig_list[i].FullFileName]))
                                listView_dubl_files.Groups.Add((ListViewGroup)TableGroups[orig_list[i].FullFileName]);
                            dubl_list[k].Dubl = true;
                            ListViewItem dublfile = new ListViewItem(dubl_list[k].FullFileName);
                            dublfile.SubItems.Add(dubl_list[k].FiLen.ToString("### ### ### ##0"));
                            dublfile.Group = (ListViewGroup)TableGroups[orig_list[i].FullFileName];
                            listView_dubl_files.Items.Add(dublfile);
                            listView_dubl_files.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                        }
                    }
                }
            }
        }
        private void ListView_dubl_files_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listView_dubl_files.CheckedItems.Count > 0) button_del.Enabled = true;
            else button_del.Enabled = false;
        }
        private void BackgroundWorker_orig_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            IEnumerable<FileInfo> workfiles = (IEnumerable<FileInfo>)e.Argument;
            int readyfiles = 1; //Обработанных файлов
            try //Заполнили всеми файлами оригинал
            {
                foreach (FileInfo WF in workfiles)
                {
                    File_Struct file = new File_Struct
                    {
                        Dubl = false,
                        FullFileName = WF.FullName,
                        HashCodeFile = BitConverter.ToString(HashFile(WF.FullName)),
                        FiLen = WF.Length
                    };
                    orig_list.Add(file);
                    worker.ReportProgress(readyfiles * 100 / workfiles.Count(), "Обрабатывается " + readyfiles.ToString() + " файл из " + workfiles.Count().ToString() + " -> " + WF.Name);
                    readyfiles++;
                    Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BackgroundWorker_orig_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar_search.Value = e.ProgressPercentage;
            toolStripStatusLabel_search.Text = e.UserState.ToString();
        }

        private void BackgroundWorker_orig_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar_search.Value = 0;
            CheckDubs_SinglePath(orig_list); //Проверяем на дублинаты оригинал
            toolStripStatusLabel_search.Text = $"Обработано {orig_list.Count} файлов. Найдено {listView_dubl_files.Items.Count} дублей.";
        }

        private void BackgroundWorker_dubl_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            IEnumerable<FileInfo> workfiles = (IEnumerable<FileInfo>)e.Argument;
            int readyfiles = 1; //Обработанных файлов
            try //Заполнили всеми файлами дублями
            {
                foreach (FileInfo WF in workfiles)
                {
                    File_Struct file = new File_Struct
                    {
                        Dubl = false,
                        FullFileName = WF.FullName,
                        HashCodeFile = BitConverter.ToString(HashFile(WF.FullName)),
                        FiLen = WF.Length
                    };
                    dubl_list.Add(file);
                    worker.ReportProgress(readyfiles * 100 / workfiles.Count(), "Обрабатывается " + readyfiles.ToString() + " файл из " + workfiles.Count().ToString() + " -> " + WF.Name);
                    readyfiles++;
                    Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BackgroundWorker_dubl_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar_search.Value = e.ProgressPercentage;
            toolStripStatusLabel_search.Text = e.UserState.ToString();
        }

        private void BackgroundWorker_dubl_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar_search.Value = 0;
            CheckDubs_SinglePath(dubl_list); //Проверяем на дублинаты дубликаты
            toolStripStatusLabel_search.Text = $"Обработано {dubl_list.Count} файлов. Найдено {listView_dubl_files.Items.Count} дублей.";
        }

        private void Button_orig_Click(object sender, EventArgs e)
        {
            orig_list.Clear();
            if (folderBrowserDialog_orig.ShowDialog() == DialogResult.OK)
            {
                button_orig.Text = folderBrowserDialog_orig.SelectedPath;
                IEnumerable<FileInfo> workF = new DirectoryInfo(folderBrowserDialog_orig.SelectedPath).EnumerateFiles("*.*", SearchOption.AllDirectories);
                if (!backgroundWorker_orig.IsBusy) backgroundWorker_orig.RunWorkerAsync(workF);
            }
            else
            {
                button_orig.Text = "Выберите папку-оригинал";
                folderBrowserDialog_orig.SelectedPath = string.Empty;
            }
        }

        private void Button_dubl_Click(object sender, EventArgs e)
        {
            dubl_list.Clear();
            if (folderBrowserDialog_dubl.ShowDialog() == DialogResult.OK)
            {
                button_dubl.Text = folderBrowserDialog_dubl.SelectedPath;
                IEnumerable<FileInfo> workF = new DirectoryInfo(folderBrowserDialog_dubl.SelectedPath).EnumerateFiles("*.*", SearchOption.AllDirectories);
                if (!backgroundWorker_dubl.IsBusy) backgroundWorker_dubl.RunWorkerAsync(workF);
            }
            else
            {
                button_dubl.Text = "Выберите папку-дубликаты";
                folderBrowserDialog_dubl.SelectedPath = string.Empty;
            }
        }

        private void Button_exe_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel_search.Text = "Обрабатываем ...";
            CheckDubs();
            toolStripStatusLabel_search.Text = $"Обработано {orig_list.Count + dubl_list.Count} файлов. Найдено {listView_dubl_files.Items.Count} дублей.";
        }

        private void Button_del_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Удаляем отмеченные файлы",
                "Подтверждение удаления файлов!",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                foreach (ListViewItem item in listView_dubl_files.CheckedItems)
                {
                    try
                    {
                        File.Delete(item.Text);
                        listView_dubl_files.Items.RemoveAt(item.Index);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                button_del.Enabled = false;
            }
            else foreach (ListViewItem item in listView_dubl_files.CheckedItems)
                {
                    item.Checked = false;
                }
        }

        private void ПоменятьМестамиОригиналИДубликатToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView_dubl_files.SelectedItems.Count > 0)
            {
                string conversion = listView_dubl_files.SelectedItems[0].Text;
                CreateTableGroups(conversion);
                listView_dubl_files.SelectedItems[0].Text = listView_dubl_files.SelectedItems[0].Group.Header;
                listView_dubl_files.SelectedItems[0].Group.Header = conversion;
            }
        }

        private void ContextMenuStrip_dubl_files_Opening(object sender, CancelEventArgs e)
        {
            if (listView_dubl_files.Items.Count > 0) поменятьМестамиОригиналИДубликатToolStripMenuItem.Enabled = true;
            else поменятьМестамиОригиналИДубликатToolStripMenuItem.Enabled = false;
        }
        #endregion
    }

    /// <summary>
    /// Структура файла
    /// </summary>
    class File_Struct
    {
        public bool Dubl { get; set; }
        public string FullFileName { get; set; }
        public string HashCodeFile { get; set; }
        public long FiLen { get; set; }
        public File_Struct(bool dubl, string fullfilename, string hashcodefile, long filen)
        {
            Dubl = dubl;
            FullFileName = fullfilename;
            HashCodeFile = hashcodefile;
            FiLen = filen;
        }
        public File_Struct() { }
    }
}

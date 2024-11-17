using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Correlation.States;

namespace Correlation
{
    public partial class MainWindow : Form
    {
        /// <summary>
        /// Обработчик файла образов.
        /// </summary>
        private ImageFileHandler Handler;

        /// <summary>
        /// Делегат для изменения состояния обработчика событий калькулятора.
        /// </summary>
        /// <param name="state">Экземпляр нового состояния.</param>
        private delegate void ChangeHandlerState(BaseState state);

        /// <summary>
        /// Делегат для записи значения в ячейку таблицы для матрицы коэффициентов.
        /// </summary>
        /// <param name="cell_value"></param>
        private delegate void SetCellInMatrix(double value, int i, int j);

        /// <summary>
        /// Параллельный поток для выполнения вычислений.
        /// </summary>
        private Thread CalculationThread;

        public MainWindow()
        {
            InitializeComponent();

            Handler = new ImageFileHandler();
            Handler.onStateChange += StateChangingHandle;
            StateStrip.Items.Add(Handler.GetState());

            KeyView.Items.Clear();
            KeyView.Columns.Clear();

            KeyView.Columns.Add("№", 28);
            for (int i = 0; i < 2; i++)
                KeyView.Columns.Add($"{i + 1}", 18);
        }

        /// <summary>
        /// Запись ячейки таблицы для матрицы коэффициентов.
        /// </summary>
        /// <param name="value">Значение ячейки</param>
        /// <param name="i">Столбец</param>
        /// <param name="j">Строка</param>
        private void SetCellInMatrixTable(double value, int i, int j)
        {
            CorrelationTable[i, j].Value = value.ToString();
        }

        /// <summary>
        /// Инициализация переключателей ключей.
        /// </summary>
        private void InitKeySelectors()
        {
            FirstKeySelecter.Items.Clear();
            SecondKeySelecter.Items.Clear();

            for (int i = 1; i <= Handler.Size(); i++)
            {
                FirstKeySelecter.Items.Add(i.ToString());
                SecondKeySelecter.Items.Add(i.ToString());
            }

            if (Handler.Size() > 0)
                FirstKeySelecter.SelectedIndex = 0;

            if (Handler.Size() > 1)
                SecondKeySelecter.SelectedIndex = 1;
        }

        /// <summary>
        /// Обработка выбора ключа.
        /// </summary>
        private void HandleKeySelectors()
        {
            if (FirstKeySelecter.SelectedIndex == SecondKeySelecter.SelectedIndex)
                MessageBox.Show("Ошибка! Этот ключ уже выбран.");

            Int16 first_index = Int16.Parse(FirstKeySelecter.SelectedItem != null ? FirstKeySelecter.SelectedItem.ToString() : "1");
            Int16 second_index = Int16.Parse(SecondKeySelecter.SelectedItem != null ? SecondKeySelecter.SelectedItem.ToString() : "2");

            ShowKeyPairView(first_index, second_index);
        }

        /// <summary>
        /// Поразрядно выводит выбранную пару ключей.
        /// Индексация с единицы! Из соображений интуитивно понятного интерфейса.
        /// </summary>
        /// <param name="first_image_id">Индекс первого ключа.</param>
        /// <param name="second_image_id">Индекс второго ключа.</param>
        private void ShowKeyPairView(Int16 first_image_id, Int16 second_image_id)
        {
            try
            {
                KeyView.Items.Clear();

                // Выставляем заголовки в соответствии с номером выбранных ключей.
                KeyView.Columns[1].Text = FirstKeySelecter.Text;
                KeyView.Columns[2].Text = SecondKeySelecter.Text;

                String[] key_1 = Handler.ImageList[first_image_id - 1].GetKeyBitsStringArray();
                String[] key_2 = Handler.ImageList[second_image_id - 1].GetKeyBitsStringArray();

                // Побитово выводим ключи.
                for (int i = 1; i <= 256; i++)
                {
                    var new_bits_view_item = new ListViewItem(new String[] {
                        i.ToString(),
                        Handler.Size() > 0 ? key_1[i - 1] : "",
                        Handler.Size() > 1 ? key_2[i - 1] : "",
                    });

                    // Биты совпали? Отмечаем цветом.
                    new_bits_view_item.BackColor =
                        key_1[i - 1] == key_2[i - 1] ? Color.Green : Color.White;

                    new_bits_view_item.ForeColor =
                        key_1[i - 1] == key_2[i - 1] ? Color.White : Color.Black;

                    KeyView.Items.Add(new_bits_view_item);
                }

                KeyView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
                KeyView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.HeaderSize);
            }

            catch (System.ArgumentOutOfRangeException)
            {
                return;
            }
        }

        /// <summary>
        /// Обработка события смены состояний обработчика.
        /// </summary>
        private void StateChangingHandle()
        {
            // Отобразить новое состояние.
            StateStrip.Items[0].Text = Handler.GetState();

            if (!(Handler.State is CalculatingState) && !(Handler.State is SavingState))
            {
                if (Handler.GetState() == Handler.StatePool[1].StateName)
                    this.Text = $"Калькулятор корреляции (образов: {Handler.Size()})";

                else if (Handler.GetState() == Handler.StatePool[2].StateName)
                    this.Text = $"Калькулятор корреляции (ошибка)";

                else
                    this.Text = $"Калькулятор корреляции";

                // Обновить список ключей.
                KeyListView.Items.Clear();

                for (int i = 0; i < Handler.Size(); i++)
                    KeyListView.Items.Add(new ListViewItem(new String[] {
                        (i+1).ToString(),
                        Handler.ImageList[i].ExceptedValue().ToString(),
                        Handler.ImageList[i].StandardDeviation().ToString(),
                        Handler.ImageList[i].BinaryKey
                    }));

                // Резервируем строки и столбцы таблицы.
                CorrelationTable.ColumnCount = Handler.Size();
                CorrelationTable.RowCount = Handler.Size();

                // Нумерация матрицы коэффициентов.
                for (int i = 1; i <= Handler.Size(); i++)
                {
                    CorrelationTable.Columns[i - 1].HeaderText = i.ToString();
                    CorrelationTable.Rows[i - 1].HeaderCell.Value = i.ToString();
                }
            }
        }

        /// <summary>
        /// Обработчик перехода на вкладку. При открытии вкладки с матрицей, пересчитывает и выводит эту матрицу на экран.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0) 
            {
                if (CalculationThread != null &&
                        CalculationThread.ThreadState != ThreadState.Unstarted &&
                        CalculationThread.ThreadState != ThreadState.Aborted &&
                        CalculationThread.ThreadState != ThreadState.Stopped)
                    
                    CalculationThread.Suspend();
            }

            else if (tabControl1.SelectedIndex == 1)
            {
                if (CalculationThread != null && CalculationThread.ThreadState == ThreadState.Suspended)
                    CalculationThread.Resume();

                else
                {

                    CalculationThread = new Thread(() =>
                    {
                        States.CalculatingState calculating_state = (States.CalculatingState)Handler.StatePool[3];

                        Handler.SetNewState(calculating_state);

                        // Вычисляем коэффициенты корреляции всех прочитанных ключей между собой.
                        for (int i = 0; i < Handler.Size(); i++)
                        {
                            for (int j = 0; j < Handler.Size(); j++)
                            {

                                if (i >= j)
                                    this.Invoke(
                                        new SetCellInMatrix(SetCellInMatrixTable),
                                        CoefficientCalculator.Compare(
                                            Handler.ImageList[i],
                                            Handler.ImageList[j]),
                                        i,
                                        j
                                    );

                            }

                            if (Thread.CurrentThread.ThreadState != ThreadState.AbortRequested &&
                                Thread.CurrentThread.ThreadState != ThreadState.Aborted)
                            {
                                calculating_state.Percent = Math.Round(
                                    ((double)(i * Handler.Size()) / Math.Pow(Handler.Size(), 2)) * 100, 1);

                                this.Invoke(new ChangeHandlerState(Handler.SetNewState), calculating_state);
                            }
                        }

                        calculating_state.Percent = 100;
                        this.Invoke(new ChangeHandlerState(Handler.SetNewState), calculating_state);
                    });

                    CalculationThread.Start();
                }
            }
        }

        /// <summary>
        /// Обработка пункта меню для открытия файла.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "TXT-файлы (*.txt) | *.txt";
            openFileDialog.Title = "Открыть файл образов";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var fileContent = string.Empty;

            // Запускаем диалог чтения файла.
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileStream = openFileDialog.OpenFile();

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    // Читаем данные из выбранного файла.
                    fileContent = reader.ReadToEnd();

                    // Передаем данные из файла в обработчик.
                    Handler.HandleImageFileData(fileContent);

                    // Инициализация переключателей ключей.
                    InitKeySelectors();

                    // Вывод ключей.
                    ShowKeyPairView(1, 2);

                    if (Handler.Size() == 1)
                        MessageBox.Show("Предупреждение: файл содержит только один ключ.");
                }
            }
        }

        /// <summary>
        /// Демонстрация информации о программе.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().Show(this);
        }

        /// <summary>
        /// Вычисление коэффициента корреляции выбранной пары ключей.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalculateCorrelationButton_Click(object sender, EventArgs e)
        {
            if (Handler.Size() == 0)
            {
                MessageBox.Show("Ошибка! Вы не открыли файл.");
                return;
            }

            else if (Handler.Size() == 1)
            {
                MessageBox.Show("Ошибка! Ключ только один.");
                return;
            }

            Int16 first_index = Int16.Parse(FirstKeySelecter.SelectedItem != null ? FirstKeySelecter.SelectedItem.ToString() : "1");
            Int16 second_index = Int16.Parse(SecondKeySelecter.SelectedItem != null ? SecondKeySelecter.SelectedItem.ToString() : "2");

            MessageBox.Show($"Коэффициент корреляции ключей №{first_index} и №{second_index} равен {CoefficientCalculator.Compare(Handler.ImageList[first_index - 1], Handler.ImageList[second_index - 1])}");
        }

        /// <summary>
        /// Реагируем на изменение выбранного элемента.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FirstKeySelecter_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleKeySelectors();
        }

        /// <summary>
        /// Реагируем на изменение выбранного элемента.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SecondKeySelecter_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleKeySelectors();
        }

        /// <summary>
        /// Сохранение матрицы коеффициентов корреляции.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void сохранитьМатрицуКорреляцииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CalculationThread.ThreadState != ThreadState.Stopped)
            {
                MessageBox.Show("Ошибка! Дождитесь окончания вычисления матрицы корреляции.");
                return;
            }    

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV-файлы (*.csv) | *.csv";
            saveFileDialog.Title = "Сохранение матрицы";
            saveFileDialog.FileName = "matrix.csv";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            States.SavingState calculating_state = (States.SavingState)Handler.StatePool[4];

            // Запускаем диалог сохранения файла.
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                CalculationThread = new Thread(() =>
                {
                    string csv = string.Empty;

                    //Add the Header row for CSV file.
                    foreach (DataGridViewColumn column in CorrelationTable.Columns)
                    {
                        csv += column.HeaderText + ',';
                    }

                    //Add new line.
                    csv += "\r\n";

                    int iters = CorrelationTable.RowCount * CorrelationTable.ColumnCount + 1;
                    int iter = 0;

                    //Adding the Rows
                    foreach (DataGridViewRow row in CorrelationTable.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            try
                            {
                                csv += cell.Value.ToString().Replace(",", ".") + ',';
                            }

                            catch (System.NullReferenceException)
                            {
                                csv += ',';
                            }

                            iter++;
                        }

                        //Add new line.
                        csv += "\r\n";

                        calculating_state.Percent = Math.Round(((double)iter / (double)iters) * 100, 1);
                        this.Invoke(new ChangeHandlerState(Handler.SetNewState), calculating_state);
                    }

                    File.WriteAllText(saveFileDialog.FileName, csv);

                    calculating_state.Percent = 100;
                    this.Invoke(new ChangeHandlerState(Handler.SetNewState), calculating_state);
                });

                CalculationThread.Start();
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (CalculationThread.ThreadState == ThreadState.Running)
                    CalculationThread.Abort();
            }

            catch { }
        }
    }
}

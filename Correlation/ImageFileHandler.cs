using System;
using System.Collections.Generic;
using Correlation.States;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace Correlation
{
    /// <summary>
    /// Класс, описывающий файл ключей.
    /// </summary>
    public class ImageFileHandler
    {
        /// <summary>
        /// Список образов, прочитанных из файла.
        /// </summary>
        public List<Image> ImageList = new List<Image>();

        /// <summary>
        /// Пул возможных состояний обработчика.
        /// [0] Файл не открыт;
        /// [1] Файл успешно открыт и прочитан;
        /// [2] Ошибка при чтении файла;
        /// [3] Идёт расчёт матрицы коэффициентов;
        /// [4] Идет сохранение матрицы коэффициентов.
        /// </summary>
        public List<BaseState> StatePool = new List<BaseState> {
            new NotOpenedState(),
            new OpenedState(),
            new ErrorState(),
            new CalculatingState(),
            new SavingState(),
        };

        public ImageFileHandler()
        {
            // Начальное состояние: файл не открыт.
            State = StatePool[0];
        }

        public delegate void StateContainer();

        /// <summary>
        /// Событие смены состояния обработчика.
        /// </summary>
        public event StateContainer onStateChange;

        /// <summary>
        /// Текущее состояние обработчика.
        /// </summary>
        public BaseState State;

        /// <summary>
        /// Регулярное выражение для поиска бинарного ключа.
        /// </summary>
        private String BinaryKeyPattern = @"[01]{256}";

        /// <summary>
        /// Переопределение метода Size для получения количества открытых образов.
        /// </summary>
        /// <returns>Количество образов, прочитанных из файла.</returns>
        public Int32 Size()
        {
            return ImageList.Count;
        }

        /// <summary>
        /// Сменить состояние.
        /// </summary>
        public void SetNewState(BaseState state)
        {
            State = state;
            onStateChange();
        }

        /// <summary>
        /// Вернуть текущее состояние.
        /// </summary>
        public String GetState()
        {
            return State.StateName;
        }

        /// <summary>
        /// Обработка содержимого файла образов.
        /// </summary>
        /// <param name="InitialData">Текстовые данные, полученные из файла.</param>
        public void HandleImageFileData(String InitialData)
        {
            try
            {
                ImageList.Clear();

                if (Regex.Match(InitialData, BinaryKeyPattern).Success)
                {
                    foreach (Match m in Regex.Matches(InitialData, BinaryKeyPattern))
                    {
                        // По совпадению регулярки создаем очередной объект образа.
                        Image img = new Image();
                        img.BinaryKey = m.Value;
                        ImageList.Add(img);
                    }

                    SetNewState(StatePool[1]);
                }

                else
                    throw new Exception("Файл не содержит двоичных ключей.");
            }

            catch (Exception e)
            {
                // Устанавливаем ошибочное состояние.
                SetNewState(StatePool[2]);

                MessageBox.Show(e.Message);
            }
        }
    }
}

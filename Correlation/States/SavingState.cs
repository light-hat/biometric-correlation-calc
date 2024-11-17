using System;

namespace Correlation.States
{
    /// <summary>
    /// Класс, описывающий состояние обработчика файла образов.
    /// </summary>
    public class SavingState : BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public override String StateName => $"Сохранение матрицы корреляции ({Percent}%)";

        /// <summary>
        /// Процент, сколько данных сохранено.
        /// </summary>
        public Double Percent = 0;
    }
}

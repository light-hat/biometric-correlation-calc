using System;

namespace Correlation.States
{
    /// <summary>
    /// Класс, описывающий состояние обработчика файла образов.
    /// </summary>
    public class CalculatingState : BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public override String StateName => $"Пересчёт матрицы корреляции ({Percent}%)";

        /// <summary>
        /// Процент, сколько данных прочитано.
        /// </summary>
        public Double Percent = 0;
    }
}

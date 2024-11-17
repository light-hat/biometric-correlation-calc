using System;

namespace Correlation.States
{
    /// <summary>
    /// Класс, описывающий состояние обработчика файла образов.
    /// </summary>
    public class NotOpenedState : BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public override String StateName => "Файл образов не выбран.";
    }
}

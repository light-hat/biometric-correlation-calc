using System;

namespace Correlation.States
{
    /// <summary>
    /// Класс, описывающий состояние обработчика файла образов.
    /// </summary>
    public class OpenedState : BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public override String StateName => "Файл образов успешно открыт.";
    }
}

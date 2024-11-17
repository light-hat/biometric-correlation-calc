using System;

namespace Correlation.States
{
    /// <summary>
    /// Класс, описывающий состояние обработчика файла образов.
    /// </summary>
    public class ErrorState : BaseState
    {
        /// <summary>
        /// Имя конкретного состояния.
        /// </summary>
        public override String StateName => "Произошла ошибка при чтении файла.";
    }
}

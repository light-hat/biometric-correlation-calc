using Microsoft.VisualStudio.TestTools.UnitTesting;
using Correlation;
using System;

namespace CorrelationCalcUnitTests
{
    [TestClass]
    public class MathOperationsTest
    {
        /// <summary>
        /// Тестовые данные файла ключа.
        /// </summary>
        private static String KeyFileData = @"Cимвольный пароль:
        Kq5fwMDev8xqjngHVZ63lInNgM8HcYbO
        Шестнадцатиричный ключ:
        4B713566774D4465763878716A6E6748565A36336C496E4E674D38486359624F
        Двоичный ключ:
        0100101101110001001101010110011001110111010011010100010001100101011101100011100001111000011100010110101001101110011001110100100001010110010110100011011000110011011011000100100101101110010011100110011101001101001110000100100001100011010110010110001001001111

        ";

        private static String InvalidFileData = @"В три сигма попало 415 параметров из 416
        1
        1
        1
        1
        1
        1
        1";
        
        /// <summary>
        /// Тестовый экземпляр класса-обработчика.
        /// </summary>
        private static ImageFileHandler Handler = new ImageFileHandler();

        /// <summary>
        /// Тестирование наличия изначального состояния.
        /// </summary>
        /// <param name="context"></param>
        [TestMethod()]
        public void InitialStateTest(TestContext context)
        {
            Assert.AreEqual("Файл образов не выбран.", Handler.GetState());
        }

        /// <summary>
        /// Тестирование переключения на ошибочное состояние при неверных данных.
        /// </summary>
        /// <param name="context"></param>
        [TestMethod()]
        public void ErrorStateTest()
        {
            Handler.HandleImageFileData(InvalidFileData);
            Assert.AreEqual("Произошла ошибка при чтении файла.", Handler.GetState());
        }

        /// <summary>
        /// Тестирование открытия файла и переключения состояния.
        /// </summary>
        /// <param name="context"></param>
        [TestMethod()]
        public void OpenedStateTest()
        {
            Handler.HandleImageFileData(KeyFileData);
            Assert.AreEqual("Файл образов успешно открыт.", Handler.GetState());
        }

        /// <summary>
        /// Проверяет длину списка образов, в тестовых данных образ один.
        /// </summary>
        [TestMethod]
        public void ImageListSizeTest()
        {
            Assert.AreEqual(1, Handler.Size());
        }


    }
}

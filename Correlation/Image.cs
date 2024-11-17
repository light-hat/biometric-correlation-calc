using System;
using System.Collections.Generic;
using System.Linq;

namespace Correlation
{
    /// <summary>
    /// Структура, описывающая образ.
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Двоичное представление ключа образа.
        /// </summary>
        public string BinaryKey { get; set; }

        /// <summary>
        /// Преобразует ключ из строкового вида в List целых чисел.
        /// </summary>
        /// <returns>Структура данных List c битами ключа.</returns>
        public List<Double> GetKeyBits()
        {
            List<Double> key_bits = new List<Double>(256);

            foreach (Char c in BinaryKey)
                key_bits.Add(Double.Parse(c.ToString()));

            return key_bits;
        }

        /// <summary>
        /// Преобразует ключ из строкового вида в массив строк.
        /// </summary>
        /// <returns>Массив строк с битами ключа.</returns>
        public String[] GetKeyBitsStringArray()
        {
            List<String> key_bits = new List<String>(256);

            foreach (Char c in BinaryKey)
                key_bits.Add(c.ToString());

            return key_bits.ToArray();
        }

        /// <summary>
        /// Метод вычисления математического ожидания для конкретного образа.
        /// </summary>
        /// <returns>Вычисленное значение математического ожидания.</returns>
        public Double ExceptedValue()
        {
            return Math.Round(GetKeyBits().Sum() / 256, 3);
        }

        /// <summary>
        /// Метод вычисления стандартного отклонения для конкретного образа.
        /// </summary>
        /// <returns>Вычисленное значение стандартного отклонения.</returns>
        public Double StandardDeviation()
        {
            Double x_i_sum = 0;
            Double excepted_value = ExceptedValue();

            foreach (Double b in GetKeyBits())
                x_i_sum += Math.Pow(b - excepted_value, 2);

            return Math.Round(Math.Sqrt(x_i_sum / 255), 3);
        }
    }
}

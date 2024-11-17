using System;
using System.Linq;

namespace Correlation
{
    /// <summary>
    /// Статический класс для вычисления коэффициентов корреляции.
    /// </summary>
    public static class CoefficientCalculator
    {
        /// <summary>
        /// Метод класса, сравнивающий два двоичных ключа, сгенерированных нейросетью.
        /// </summary>
        /// <param name="first">Первый ключ, X, с которым идёт сравнение.</param>
        /// <param name="second">Второй ключ, Y.</param>
        /// <returns>Коэффициент корреляции ключей.</returns>
        public static Double Compare(Image first, Image second)
        {
            // Вычисляем математическое ожидание входных ключей.
            Double first_excepted_value = first.ExceptedValue();
            Double second_excepted_value = second.ExceptedValue();

            // Вычисляем стандартное отклонение входных ключей.
            Double first_standard_deviation = first.StandardDeviation();
            Double second_standard_deviation = second.StandardDeviation();

            // Если ключи совпадают, коэффициент равен 1.
            if (first.BinaryKey == second.BinaryKey)
                return 1;

            // Вычисляем коэффициент корреляции по формуле.
            return Math.Round((1.0 / 256.0) * first.GetKeyBits().Zip(
                second.GetKeyBits(),
                (x_i, y_i) => 
                ((x_i - first_excepted_value) * (y_i - second_excepted_value)) / 
                (first_standard_deviation * second_standard_deviation)).Sum(), 3);
        }
    }
}

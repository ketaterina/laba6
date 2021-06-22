//Лаб 6.
//Разработайте .NET приложения обменивающиеся данными структур и классов, используя базовую бинарную, XML и JSON сериализацию.

using System;
using System.Windows.Forms;

namespace App
{

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main ()
        {
            Application.EnableVisualStyles ();
            Application.SetCompatibleTextRenderingDefault (false);
            Application.Run (new Form1 ());
        }
    }
}

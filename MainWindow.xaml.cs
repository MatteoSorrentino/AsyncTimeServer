using Sorrentino_SocketAsyncLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sorrentino_AsyncTimeServer
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AsyncSocketServer mServer;
        DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            mServer = new AsyncSocketServer();
        }
        private void btn_Ascolta_Click(object sender, RoutedEventArgs e)
        {
            mServer.StartListening();

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            mServer.SendToAll(DateTime.Now.ToShortDateString() + " ");
            mServer.SendToAll(DateTime.Now.ToShortTimeString());
        }

        private void btn_Disconetti_Click(object sender, RoutedEventArgs e)
        {
            mServer.CloseConnection();
        }

        private void btn_Invia_Click(object sender, RoutedEventArgs e)
        {
            mServer.SendToAll(txt_Messaggio.Text);
        }
    }
}

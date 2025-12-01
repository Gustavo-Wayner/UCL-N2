using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UCL_N2
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
        }

        private void AddUser(object sender, RoutedEventArgs e)
        {
            AddUserWindow win = new AddUserWindow();
            win.Show();
            this.Close();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            Atual.Usuario = null;
            LoginWindow win = new LoginWindow();
            win.Show();
            this.Close();
        }

        private void AddSubject(object sender, RoutedEventArgs e)
        {
            AddSubjectWindow win = new AddSubjectWindow();
            win.Show();
            this.Close();
        }

        private void AddToSubject(object sender, RoutedEventArgs e)
        {
            EnrollWindow win = new EnrollWindow();
            win.Show();
            this.Close();
        }
    }
}

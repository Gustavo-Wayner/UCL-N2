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
    public partial class AddUserWindow : Window
    {
        public ObservableCollection<Cadastro> cadastros { get; } = new();

        public List<string> Papeis { get; } = new()
        {
            "",
            "Admin",
            "Professor",
            "Aluno"
        };

        public AddUserWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadUsers();

            PapelDropDown.ItemsSource = Papeis;
            PapelDropDown.SelectedIndex = 0;
        }

        public void LoadUsers()
        {
            cadastros.Clear();

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cadastros";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Cadastro c = new Cadastro
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Papel = reader.GetString(2)
                };

                cadastros.Add(c);
            }
        }

        private void OnPressAdd(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Input.Text.Trim()) || PapelDropDown.SelectedIndex == 0)
            {
                MessageBox.Show("Preencha ambos os campos!");
                return;
            }

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Cadastros (Nome, Papel) VALUES ($nome, $papel);";
            command.Parameters.AddWithValue("$nome", Input.Text.Trim());
            command.Parameters.AddWithValue("$papel", PapelDropDown.SelectedValue);

            command.ExecuteNonQuery();
            PapelDropDown.SelectedIndex = 0;
            Input.Text = string.Empty;
            LoadUsers();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            Atual.Usuario = null;
            LoginWindow win = new LoginWindow();
            win.Show();
            this.Close();
        }

        public void OnEditPapel(object sender, SelectionChangedEventArgs e)
        {
            // Garantir que veio de um ComboBox da linha
            if (sender is not ComboBox combo || combo.DataContext is not Cadastro selected)
                return;

            // Novo papel selecionado
            string? novoPapel = combo.SelectedItem as string;

            // Papel ANTERIOR (vem do SelectionChanged)
            string? papelAnterior = e.RemovedItems.Count > 0
                ? e.RemovedItems[0] as string
                : selected.Papel; // fallback

            if (string.IsNullOrWhiteSpace(novoPapel))
            {
                MessageBox.Show("Papel inválido!");
                combo.SelectedItem = papelAnterior;
                return;
            }

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            // Se está tirando o papel de Admin de alguém
            if (papelAnterior == "Admin" && novoPapel != "Admin")
            {
                using var cmdCount = connection.CreateCommand();
                cmdCount.CommandText = "SELECT COUNT(*) FROM Cadastros WHERE Papel = 'Admin';";
                long admins = (long)cmdCount.ExecuteScalar()!;

                // Se só existia esse admin, não deixa mudar
                if (admins <= 1)
                {
                    MessageBox.Show("É necessário que haja no mínimo um admin!");

                    // Volta visualmente e no objeto
                    combo.SelectedItem = papelAnterior;
                    selected.Papel = papelAnterior;
                    return;
                }
            }

            // Atualiza o objeto em memória
            selected.Papel = novoPapel;

            // Atualiza o banco
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Cadastros SET Papel = $papel WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$papel", novoPapel);
            cmd.Parameters.AddWithValue("$id", selected.Id);
            cmd.ExecuteNonQuery();
        }
    }
}

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

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cadatros
                (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome        TEXT NOT NULL,
                    Papel       TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();

            PapelDropDown.ItemsSource = Papeis;
            PapelDropDown.SelectedIndex = 0;
        }

        public void LoadUsers()
        {
            cadastros.Clear();

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cadastros ORDER BY Papel";

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

            Input.Text = Persistent.TitleCase(Input.Text.Trim());

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Cadastros WHERE Papel = 'Aluno' AND Nome = $nome;";
                cmd.Parameters.AddWithValue("$nome", Input.Text);
                using SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    MessageBox.Show("Nomes repetidos não são permitidos!");
                    return;
                }
            }

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Cadastros (Nome, Papel) VALUES ($nome, $papel);";
            command.Parameters.AddWithValue("$nome", Input.Text);
            command.Parameters.AddWithValue("$papel", PapelDropDown.SelectedValue);

            command.ExecuteNonQuery();
            PapelDropDown.SelectedIndex = 0;
            Input.Text = string.Empty;
            LoadUsers();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            Persistent.Usuario = null;
            LoginWindow win = new LoginWindow();
            win.Show();
            this.Close();
        }

        public void OnEditPapel(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox combo || combo.DataContext is not Cadastro selected)
                return;

            string? novoPapel = combo.SelectedItem as string;

            string? papelAnterior = e.RemovedItems.Count > 0
                ? e.RemovedItems[0] as string
                : selected.Papel;

            if (string.IsNullOrWhiteSpace(novoPapel))
            {
                MessageBox.Show("Papel inválido!");
                combo.SelectedItem = papelAnterior;
                return;
            }

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            if (papelAnterior == "Admin" && novoPapel != "Admin")
            {
                using var cmdCount = connection.CreateCommand();
                cmdCount.CommandText = "SELECT COUNT(*) FROM Cadastros WHERE Papel = 'Admin';";
                long admins = (long)cmdCount.ExecuteScalar()!;

                if (admins <= 1)
                {
                    MessageBox.Show("É necessário que haja no mínimo um admin!");

                    combo.SelectedItem = papelAnterior;
                    selected.Papel = papelAnterior;
                    return;
                }
            }

            selected.Papel = novoPapel;

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Cadastros SET Papel = $papel WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$papel", novoPapel);
            cmd.Parameters.AddWithValue("$id", selected.Id);
            cmd.ExecuteNonQuery();
        }

        private void OnDelPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                var selected = GridCadastros.SelectedItem as Cadastro;
                if (selected == null) return;

                using var connection = new SqliteConnection("Data Source=tables.db");
                connection.Open();

                using var tx = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.Transaction = tx;
                command.CommandText = "SELECT COUNT(*) FROM Cadastros WHERE Papel = 'Admin';";
                long admins = (long)command.ExecuteScalar()!;
                if (admins == 1 && selected!.Papel == "Admin")
                {
                    MessageBox.Show("É necessário que haja no mínimo um admin!");
                    return;
                }

                command.CommandText = @"
                    DELETE FROM Matriculas
                    WHERE MateriaId IN (SELECT Id FROM Materias WHERE ProfessorId = $id);
                ";
                command.Parameters.AddWithValue("$id", selected!.Id);
                command.ExecuteNonQuery();

                command.CommandText = "DELETE FROM Materias WHERE ProfessorId = $id;";
                command.ExecuteNonQuery();

                command.CommandText = "DELETE FROM Cadastros WHERE Id = $id;";
                command.ExecuteNonQuery();

                tx.Commit();
                LoadUsers();
            }
        }

        private void OnReturn(object sender, RoutedEventArgs e)
        {
            AdminWindow win = new();
            win.Show();
            this.Close();
        }
    }
}

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UCL_N2
{
    public partial class AddSubjectWindow : Window
    {
        public ObservableCollection<Materia> materias { get; } = new();
        public AddSubjectWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadSubjects();

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Materias
                (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    Titulo      TEXT NOT NULL,
                    Professor   TEXT NOT NULL,
                    Turma       TEXT NOT NULL
                );
            ";
            command.ExecuteNonQuery();
        }

        private void OnPressAdd(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(InputTitulo.Text.Trim()) || string.IsNullOrWhiteSpace(InputProf.Text.Trim()) || string.IsNullOrWhiteSpace(InputTurma.Text.Trim()))
            {
                MessageBox.Show("Preencha todos o campos para adicionar!");
                return;
            }


            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using(var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Cadastros WHERE Nome = $nome AND Papel = 'Professor'";
                cmd.Parameters.AddWithValue("$nome", InputProf.Text.Trim());

                using var reader = cmd.ExecuteReader();
                if(!reader.Read())
                {
                    MessageBox.Show($"Não existe nenhum(a) professor(a) chamado(a) {InputProf.Text.Trim()}!");
                    return;
                }
            }

            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Materias (Titulo, Professor, Turma) VALUES ($titulo, $prof, $turma);";

            command.Parameters.AddWithValue("$titulo", InputTitulo.Text.Trim());
            command.Parameters.AddWithValue("$prof", InputProf.Text.Trim());
            command.Parameters.AddWithValue("$turma", InputTurma.Text.Trim());

            command.ExecuteNonQuery();
            InputTitulo.Text = string.Empty;
            InputProf.Text = string.Empty;
            InputTurma.Text = string.Empty;
            LoadSubjects();
        }

        public void LoadSubjects()
        {
            materias.Clear();

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Materias";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Materia m = new Materia
                {
                    Id = reader.GetInt32(0),
                    Titulo = reader.GetString(1),
                    Professor = reader.GetString(2),
                    Turma = reader.GetString(3)
                };

                materias.Add(m);
            }
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            Atual.Usuario = null;
            LoginWindow win = new LoginWindow();
            win.Show();
            this.Close();
        }

        private void OnDelPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                var selected = GridCadastros.SelectedItem as Materia;

                using var connection = new SqliteConnection("Data Source=tables.db");
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = "DELETE FROM Materias WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", selected.Id);
                command.ExecuteNonQuery();

                LoadSubjects();
            }
        }
    }
}

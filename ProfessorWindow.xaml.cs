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
    public partial class ProfessorWindow : Window
    {
        public ObservableCollection<DadosClasse> dados = new();
        public ProfessorWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadDados();
        }
        private void OnReturn(object sender, RoutedEventArgs e)
        {
            SubjectSelectWindow win = new();
            win.Show();
            this.Close();
        }

        private void LoadDados()
        {
            dados.Clear();
            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"Select cAluno.Nome as Aluno,
                mat.FaltasPcnt,
                mat.N1, mat.P1, mat.N2, mat.P2,
                mat.Media,
                mat.Estado,
                m.ProfessorId,
                mat.AlunoId,
                mat.Id
                FROM Matriculas mat                

                JOIN Cadastros cAluno   ON mat.AlunoId = cAluno.Id
                JOIN Materias m         ON mat.MateriaId = m.Id
                WHERE m.Id = $materiaId AND cAluno.Papel = 'Aluno';
            ";
            command.Parameters.AddWithValue("$materiaId", Persistent.materia!.Id);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DadosClasse d = new DadosClasse
                {
                    Aluno = reader.GetString(0),
                    Faltas = reader.GetInt32(1),
                    N1 = reader.GetInt32(2),
                    P1 = reader.GetInt32(3),
                    N2 = reader.GetInt32(4),
                    P2 = reader.GetInt32(5),
                    Media = reader.GetInt32(6),
                    Estado = reader.GetString(7),
                    ProfessorId = reader.GetInt32(8),
                    AlunoId = reader.GetInt32(9),
                    Id = reader.GetInt32(10)
                };

                dados.Add(d);
            }
        }

        private void OnDelPress(object sender, KeyEventArgs e)
        {

        }

        private void OnAddPress(object sender, RoutedEventArgs e)
        {
            AddAluno();
        }
        private void OnKeyPress(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                e.Handled = true;
                AddAluno();
            }
        }

        private void AddAluno()
        {
            if (string.IsNullOrWhiteSpace(Input.Text.Trim())) return;
            using SqliteConnection connection = new("Data Source=tables.db");
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cadastros WHERE Papel = 'Aluno' AND Nome = $nome;";
            command.Parameters.AddWithValue("$nome", Input.Text.Trim());

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                {
                    MessageBox.Show($"Não existem alunos cadatrados com o nome de {Input.Text.Trim()}");
                    return;
                }
            }

            int Id;

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                Id = reader.GetInt32(0);
            }

            Input.Text = "";

            command.CommandText = "INSERT INTO Matriculas (Student;";
        }
    }
}

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
using System.Collections.Specialized;
using System.ComponentModel;

namespace UCL_N2
{
    public partial class ProfessorWindow : Window
    {
        public ObservableCollection<DadosClasse> dados { get; } = new();
        public ProfessorWindow()
        {
            InitializeComponent();
            DataContext = this;

            dados.CollectionChanged += Dados_CollectionChanged;

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
                mat.Id,
                mat.AlunoId,
                m.Id
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
                    Faltas = GetNullableFloat(reader, 1),
                    N1 = GetNullableFloat(reader, 2),
                    P1 = GetNullableFloat(reader, 3),
                    N2 = GetNullableFloat(reader, 4),
                    P2 = GetNullableFloat(reader, 5),
                    Media = GetNullableFloat(reader, 6),
                    Estado = reader.IsDBNull(7) ? null : reader.GetString(7),
                    MateriaId = reader.GetInt32(8),
                    AlunoId = reader.GetInt32(9),
                    Id = reader.GetInt32(10)
                };

                dados.Add(d);
            }
        }

        private void OnDelPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                var selected = GridBoletim.SelectedItem as DadosClasse;
                if (selected == null) return;

                using var connection = new SqliteConnection("Data Source=tables.db");
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = @"
                    DELETE FROM Matriculas
                    WHERE Id = $id;
                ";
                command.Parameters.AddWithValue("$id", selected.Id);
                command.ExecuteNonQuery();

                LoadDados();
            }
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

            Input.Text = Persistent.TitleCase(Input.Text.Trim());

            using SqliteConnection connection = new("Data Source=tables.db");
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cadastros WHERE Papel = 'Aluno' AND Nome = $nome;";
            command.Parameters.AddWithValue("$nome", Input.Text);

            using SqliteDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                MessageBox.Show($"Não existem alunos cadatrados com o nome de {Input.Text}");
                return;
            }

            int Id = reader.GetInt32(0);
            reader.Close();
            command.CommandText = "INSERT INTO Matriculas (MateriaId, AlunoId) VALUES ($materiaId, $alunoId);";
            command.Parameters.AddWithValue("$materiaId", Persistent.materia!.Id);
            command.Parameters.AddWithValue("$alunoId", Id);
            command.ExecuteNonQuery();

            LoadDados();
            Input.Text = string.Empty;
        }
        private float GetNullableFloat(SqliteDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? 0f : Convert.ToSingle(reader.GetDouble(ordinal));
        }

        private void Dados_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DadosClasse d in e.NewItems)
                {
                    d.PropertyChanged += Dado_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (DadosClasse d in e.OldItems)
                {
                    d.PropertyChanged -= Dado_PropertyChanged;
                }
            }
        }

        private void Dado_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not DadosClasse d)
                return;

            if (e.PropertyName is not ("Faltas" or "N1" or "P1" or "N2" or "P2" or "Media" or "Estado"))
                return;

            using var connection = new SqliteConnection("Data source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Matriculas
                SET FaltasPcnt = $faltas,
                    N1         = $n1,
                    P1         = $p1,
                    N2         = $n2,
                    P2         = $p2,
                    Media      = $media,
                    Estado     = $estado
                WHERE Id = $id;
            ";

            object DbOrNull(float? v) => v.HasValue ? (object)v.Value : DBNull.Value;

            command.Parameters.AddWithValue("$faltas", DbOrNull(d.Faltas));
            command.Parameters.AddWithValue("$n1", DbOrNull(d.N1));
            command.Parameters.AddWithValue("$p1", DbOrNull(d.P1));
            command.Parameters.AddWithValue("$n2", DbOrNull(d.N2));
            command.Parameters.AddWithValue("$p2", DbOrNull(d.P2));
            command.Parameters.AddWithValue("$media", DbOrNull(d.Media));
            command.Parameters.AddWithValue("$estado", (object?)d.Estado ?? DBNull.Value);
            command.Parameters.AddWithValue("$id", d.Id);

            command.ExecuteNonQuery();
        }

    }
}

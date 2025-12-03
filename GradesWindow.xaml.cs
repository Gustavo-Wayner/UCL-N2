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
    public partial class GradesWindow : Window
    {
        private ObservableCollection<DadosBoletim> dados = new();
        public GradesWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadDados();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            Atual.Usuario = null;
            LoginWindow win = new();
            win.Show();
            this.Close();
        }

        private void LoadDados()
        {
            dados.Clear();
            using SqliteConnection connection = new("Data Source=tables.db");
            connection.Open();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = @"
                SELECT m.Titulo, cProf.Nome as Professor, mat.FaltasPcnt,
                mat.N1, mat.P1, mat.N2, mat.P2,
                mat.Media, mat.Estado,
                m.ProfessorId, mat.Id
                FROM Matriculas mat 
                JOIN Materias m         ON mat.MateriaId = m.Id
                JOIN Cadastros cProf    ON m.ProfessorId = cProf.Id
                WHERE mat.AlunoId = $id;
            ";
            command.Parameters.AddWithValue("$id", Atual.Usuario!.Id);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                DadosBoletim d = new DadosBoletim
                {
                    Titulo = reader.GetString(0),
                    Professor = reader.GetString(1),
                    Faltas = reader.GetInt32(2),
                    N1 = reader.GetInt32(3),
                    P1 = reader.GetInt32(4),
                    N2 = reader.GetInt32(5),
                    P2 = reader.GetInt32(6),
                    Media = reader.GetInt32(7),
                    Estado = reader.GetString(8),
                    ProfessorId = reader.GetInt32(9),
                    Id = reader.GetInt32(10)
                };

                dados.Add(d);
            }
        }
    }
}

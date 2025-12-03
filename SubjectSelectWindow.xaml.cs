using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class SubjectSelectWindow : Window
    {
        public List<string> DisplaySubjects = new();
        public SubjectSelectWindow()
        {
            InitializeComponent();
            Persistent.materias = new();

            LoadTeacherSubjects();
            MateriasDropDown.ItemsSource = DisplaySubjects;
            MateriasDropDown.SelectedIndex = -1;
        }

        public void LoadTeacherSubjects()
        {
            DisplaySubjects.Clear();
            Persistent.materias?.Clear();

            using var connection = new SqliteConnection("Data source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT m.Id, m.Titulo, c.Nome AS ProfessorNome, m.Turma, m.ProfessorId
                FROM Materias m
                JOIN Cadastros c ON m.ProfessorId = c.Id
                WHERE m.ProfessorId = $id;
            ";

            command.Parameters.AddWithValue("$id", Persistent.Usuario?.Id);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Materia m = new Materia
                {
                    Id = reader.GetInt32(0),
                    Titulo = reader.GetString(1),
                    Professor = reader.GetString(2),
                    Turma = reader.GetString(3),
                    ProfessorId = reader.GetInt32(4)
                };

                Persistent.materias!.Add(m);
                DisplaySubjects.Add($"{m.Titulo} - {m.Turma}");
            }
        }

        private void OnProceed(object sender, RoutedEventArgs e)
        {
            if (MateriasDropDown.SelectedItem == null)
            {
                MessageBox.Show("Selecione uma materia antes de prosseguir!");
                return;
            }
            Console.WriteLine(MateriasDropDown.SelectedIndex);
            Persistent.materia = Persistent.materias![MateriasDropDown.SelectedIndex];
            ProfessorWindow win = new();
            win.Show();
            this.Close();
        }

        private void OnLogout(object sender, RoutedEventArgs e)
        {
            LoginWindow win = new();
            win.Show();
            this.Close();
        }
    }
}

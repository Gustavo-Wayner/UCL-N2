using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
        public ProfessorWindow()
        {
            InitializeComponent();
            DataContext = this;

            using var connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"Select cAluno as Aluno,
                mat.N1, mat.P1, mat.N2, mat.P2,
                mat.FaltasPcnt,
                mat.Media,
                mat.Estado
                FROM Matriculas mat                

                JOIN Cadastros cAluno   ON mat.AlunoId = cAluno.Id
                JOIN Materias m         ON mat.MateriaId = m.Id
                WHERE m.Id = $materiaId AND cAluno.Papel = 'Aluno';
            ";

            
        }
    }
}

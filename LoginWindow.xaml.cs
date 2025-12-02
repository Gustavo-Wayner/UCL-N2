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
using Microsoft.Data.Sqlite;

namespace UCL_N2
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Atual.materia = null;
            Atual.materias = null;
            Atual.Usuario = null;

            using SqliteConnection connection = new SqliteConnection("Data Source=tables.db");
            connection.Open();

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Cadastros
                    (
                        Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nome    TEXT NOT NULL UNIQUE,
                        Papel   TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Materias
                    (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        Titulo      TEXT NOT NULL,
                        ProfessorId  INTEGER NOT NULL,
                        Turma        TEXT NOT NULL,
                        FOREIGN KEY (ProfessorId) REFERENCES Cadastros(Id)
                    );

                    CREATE TABLE IF NOT EXISTS Matriculas
                    (
                        Id              INTEGER PRIMARY KEY AUTOINCREMENT,

                        AlunoId         INTEGER NOT NULL,
                        MateriaId       INTEGER NOT NULL,

                        N1              REAL,
                        P1              REAL,
                        N2              REAL,
                        P2              REAL,

                        FaltasPcnt   REAL,

                        Media           Real,
                        Estado          TEXT CHECK (Estado IN ('Aprovado', 'Reprovado') OR Estado IS NULL),

                        UNIQUE (AlunoId, MateriaId),

                        FOREIGN KEY (AlunoId)  REFERENCES Cadastros(Id),
                        FOREIGN KEY (MateriaId) REFERENCES Materias(Id)
                    );
                ";

                command.ExecuteNonQuery();
            }

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Cadastros;";
                long count = (long)command.ExecuteScalar()!;

                if (count == 0)
                {
                    command.CommandText = @"
                        INSERT INTO Cadastros (Nome, Papel)
                        VALUES ($u, $papel);
                    ";

                    command.Parameters.AddWithValue("$u", "Admin1");
                    command.Parameters.AddWithValue("$papel", "Admin");

                    command.ExecuteNonQuery();
                }
            }
        }

        public void OnEnter(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(Input.Text.Trim()))
                {
                    return;
                }

                e.Handled = true;

                using SqliteConnection connection = new SqliteConnection("Data Source=tables.db");
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Cadastros WHERE Nome = $name";
                command.Parameters.AddWithValue("$name", Input.Text.Trim());

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    MessageBox.Show($"O Nome {Input.Text.Trim()} não existe entre os cadastrados");
                    return;
                }

                Atual.Usuario = new Cadastro
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Papel = reader.GetString(2)
                };

                if (Atual.Usuario.Papel == "Admin")
                {
                    AdminWindow win = new();
                    win.Show();
                    this.Close();
                }

                else if(Atual.Usuario.Papel == "Aluno")
                {
                    GradesWindow win = new();
                    win.Show();
                    this.Close();
                }

                else if (Atual.Usuario.Papel == "Professor")
                {
                    SubjectSelectWindow win = new();
                    win.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Algo de errado não esta certo");
                }
            }
        }
    }
}

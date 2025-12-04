using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace UCL_N2
{
    public class Cadastro
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Papel { get; set; } = "";
    };

    public class Materia
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Professor { get; set; } = "";
        public string Turma { get; set; } = "";
        public int ProfessorId { get; set; }
    };

    public static class Persistent
    {
        public static Cadastro? Usuario;
        public static List<Materia>? materias;
        public static Materia? materia;

        public static string TitleCase(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.ToLower();
            string result = char.ToUpper(s[0]).ToString();
            for (int i = 1; i < s.Length; i++)
            {
                result += (s[i - 1] == ' ') ? char.ToUpper(s[i]) : s[i];
            }

            return result;
        }
    };

    public class DadosBoletim
    {
        public string Titulo { get; set; } = "";
        public string Professor { get; set; } = "";
        public float? Faltas { get; set; }
        public float? N1 { get; set; }
        public float? P1 { get; set; }
        public float? N2 { get; set; }
        public float? P2 { get; set; }
        public float? Media { get; set; }
        public string? Estado { get; set; } = "";
        public int ProfessorId { get; set; }
        public int Id { get; set; }

    };

    public class DadosClasse : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        string _aluno = "";
        public string Aluno
        {
            get => _aluno;
            set { _aluno = value; OnPropertyChanged(); }
        }

        float? _faltas;
        public float? Faltas
        {
            get => _faltas;
            set
            {
                if (value.HasValue && (value.Value < 0 || value.Value > 100))
                {
                    MessageBox.Show("A porcentagem de faltas precisa estar entre 0 e 100.");
                    return;
                }

                _faltas = value;
                OnNotasChanged();
                OnPropertyChanged();
            }
        }

        float? _n1;
        public float? N1
        {
            get => _n1;
            set
            {
                if (value.HasValue && (value.Value < 0 || value.Value > 10))
                {
                    MessageBox.Show("Todas as notas precisam estar entre 0 e 10.");
                    return;
                }

                _n1 = value;
                OnNotasChanged();
                OnPropertyChanged();
            }
        }

        float? _p1;
        public float? P1
        {
            get => _p1;
            set
            {
                if (value.HasValue && (value.Value < 0 || value.Value > 10))
                {
                    MessageBox.Show("Todas as notas precisam estar entre 0 e 10.");
                    return;
                }

                _p1 = value;
                OnNotasChanged();
                OnPropertyChanged();
            }
        }

        float? _n2;
        public float? N2
        {
            get => _n2;
            set
            {
                if (value.HasValue && (value.Value < 0 || value.Value > 10))
                {
                    MessageBox.Show("Todas as notas precisam estar entre 0 e 10.");
                    return;
                }

                _n2 = value;
                OnNotasChanged();
                OnPropertyChanged();
            }
        }

        float? _p2;
        public float? P2
        {
            get => _p2;
            set
            {
                if (value.HasValue && (value.Value < 0 || value.Value > 10))
                {
                    MessageBox.Show("Todas as notas precisam estar entre 0 e 10.");
                    return;
                }

                _p2 = value;
                OnNotasChanged();
                OnPropertyChanged();
            }
        }


        float? _media;
        public float? Media
        {
            get => _media;
            set { _media = value; OnPropertyChanged(); }
        }

        string? _estado;
        public string? Estado
        {
            get => _estado;
            set { _estado = value; OnPropertyChanged(); }
        }

        public int MateriaId { get; set; }
        public int AlunoId { get; set; }
        public int Id { get; set; }

        void OnNotasChanged()
        {
            if (N1.HasValue || P1.HasValue || N2.HasValue || P2.HasValue)
            {
                float n1 = N1 ?? 0;
                float p1 = P1 ?? 0;
                float n2 = N2 ?? 0;
                float p2 = P2 ?? 0;

                Media = ((n1*0.3f) + (p1*0.7f) + (n2*0.3f) + (p2*0.7f)) / 2.0f;

                if (Media >= 7 && (Faltas ?? 0) <= 25)
                    Estado = "Aprovado";
                else
                    Estado = "Reprovado";

                OnPropertyChanged(nameof(Media));
                OnPropertyChanged(nameof(Estado));
            }
            else
            {
                Media = null;
            }
            if(!(N1.HasValue && P1.HasValue && N2.HasValue && P2.HasValue))
                Estado = null;
        }
    }
}
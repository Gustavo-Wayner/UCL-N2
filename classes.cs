using System.Globalization;

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
            for(int i = 1; i < s.Length; i++)
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
        public float Faltas { get; set; }
        public float N1 { get; set; }
        public float P1 { get; set; }
        public float N2 { get; set; }
        public float P2 { get; set; }
        public float Media { get; set; }
        public string Estado { get; set; } = "";
        public int ProfessorId { get; set; }
        public int Id { get; set; }

    };

    public class DadosClasse
    {
        public string Aluno { get; set; } = "";
        public float Faltas { get; set; }
        public float N1 { get; set; }
        public float P1 { get; set; }
        public float N2 { get; set; }
        public float P2 { get; set; }
        public float Media { get; set; }
        public string Estado { get; set; } = "";
        public int ProfessorId { get; set; }
        public int AlunoId { get; set; }
        public int Id { get; set; }
    };
}
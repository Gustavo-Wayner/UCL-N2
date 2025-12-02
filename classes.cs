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
    };

    public static class Atual
    {
        public static Cadastro? Usuario;
    }
}
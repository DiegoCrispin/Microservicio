namespace AnunciosAPI.Models
{
    public class Anuncio
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public bool Urgente { get; set; }
        public bool Destacado { get; set; }

    }
}

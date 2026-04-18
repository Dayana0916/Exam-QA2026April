using System;
using System.Collections.Generic;
using System.Text;

namespace Exam_TheMovieCatalogSystem_2026QA.MovieCatalogExam.Models
{
    internal class MovieDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PosterUrl { get; set; }
        public string TrailerLink { get; set; }
        public bool IsWatched { get; set; }
    }
}

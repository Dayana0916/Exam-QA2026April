using Exam_TheMovieCatalogSystem_2026QA.MovieCatalogExam.Models;
using System.Collections.Generic;

internal class ApiResponseDTO
{
    public string msg { get; set; }
    public MovieDTO movie { get; set; }

    // Add this property so `result.movies` compiles
    public List<MovieDTO> movies { get; set; } = new List<MovieDTO>();
}
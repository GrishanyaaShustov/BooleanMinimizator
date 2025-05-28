using BooleanMinimizerLibrary;

namespace BooleanMinimizator.Models
{
    public class BooleanMinimizatorModel
    {
        public string InputFunction { get; set; }  // сюда пользователь вводит функцию

        public string ResultMessage { get; set; }  // сюда выводится результат: успех или текст ошибки
        
        public bool IsSolved { get; set; }

        public string? PolizOutput { get; set; }   // сюда выводится ПОЛИЗ

        public string? VectorOutput { get; set; }  // сюда выводится вектор функции

        public List<Dictionary<string, bool>> TruthTable { get; set; } // Таблица истинности

        public string FunctionExpression { get; set; } // Новое поле для хранения функции

        public string SKNFOutput { get; set; }      // Совершенная конъюнктивная нормальная форма

        public string SDNFOutput { get; set; }      // Совершенная дизъюнктивная нормальная форма
        
        public string MKNFOutput { get; set; }      // Минимальная конъюнктивная нормальная форма

        public string MDNFOutput { get; set; }      // Минимальная дизъюнктивная нормальная форма

        public List<List<string>> KarnaughMap { get; set; }
        
        public List<KarnaughMapBuilder.Area> Areas { get; set; }
        
    }
}

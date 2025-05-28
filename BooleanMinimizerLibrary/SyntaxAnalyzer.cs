namespace BooleanMinimizerLibrary
{
    public class SyntaxAnalyzer
    {
        private string expr;
        private char currentChar;
        private int pos;

        public Node Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Ошибка: Ввод не может быть пустым.");

            expr = input;
            pos = 0;
            NextChar();
            Node tree = Expression();
            if (currentChar != '\0')
                ThrowError("Лишние символы в конце выражения");

            return tree;
        }

        private string NodeTypeToSymbol(NodeType type) => type switch
        {
            NodeType.Not => "¬",
            NodeType.And => "∧",
            NodeType.Or => "∨",
            NodeType.Xor => "⊕",
            NodeType.Implies => "→",
            NodeType.Equivalent => "↔",
            _ => throw new Exception("Неизвестный тип узла")
        };

        private void NextChar()
        {
            currentChar = (pos < expr.Length) ? expr[pos++] : '\0';
        }

        private void ThrowError(string message)
        {
            throw new ArgumentException($"Ошибка на символе №{pos}: {message}");
        }

        private Node Expression()
        {
            Node node = Implies();
            while (currentChar == '↔')
            {
                NextChar();
                node = new Node(NodeType.Equivalent, null, node, Implies());
            }
            return node;
        }

        private Node Implies()
        {
            Node node = OrXor();
            while (currentChar == '→')
            {
                NextChar();
                node = new Node(NodeType.Implies, null, node, OrXor());
            }
            return node;
        }

        private Node OrXor()
        {
            Node node = And();
            while (currentChar == '∨' || currentChar == '⊕')
            {
                char op = currentChar;
                NextChar();
                NodeType type = op == '∨' ? NodeType.Or : NodeType.Xor;
                node = new Node(type, null, node, And());
            }
            return node;
        }

        private Node And()
        {
            Node node = Not();
            while (currentChar == '∧')
            {
                NextChar();
                node = new Node(NodeType.And, null, node, Not());
            }
            return node;
        }

        private Node Not()
        {
            if (currentChar == '¬')
            {
                NextChar();
                Node operand = Primary(); // Получаем операнд после ¬
                if (operand == null)
                    ThrowError("Ожидается переменная или константа после ¬");
                return new Node(NodeType.Not, null, null, operand);
            }
            return Primary();
        }

        private Node Primary()
        {
            if (IsVariable(currentChar))
            {
                var varName = currentChar.ToString();
                NextChar();
                return new Node(NodeType.Variable, varName);
            }
            else if (currentChar == '0' || currentChar == '1')
            {
                if (IsVector())
                {
                    string vector = ReadVector(); // Читаем строку вектора (например, "1010")

                    // Определяем количество переменных
                    int numVars = (int)Math.Log(vector.Length, 2);
                    var variables = new List<string>();
                    for (int i = 0; i < numVars; i++)
                    {
                        variables.Add(i switch
                        {
                            0 => "w",
                            1 => "x",
                            2 => "y",
                            3 => "z",
                            _ => throw new Exception("Максимум 4 переменные")
                        });
                    }

                    // Создаем узел Vector с сохраненными переменными
                    return new Node(NodeType.Vector, vector) { Variables = variables };
                }
                else
                {
                    char constant = currentChar;
                    NextChar();
                    return new Node(NodeType.Constant, constant.ToString());
                }
            }
            else if (currentChar == '(')
            {
                NextChar();
                Node node = Expression();
                if (currentChar != ')')
                    ThrowError("Ожидалось ')'");
                NextChar();
                return node;
            }
            else
            {
                ThrowError("Неожиданный символ");
                return null;
            }
        }

        private bool IsVariable(char ch) => ch == 'w' || ch == 'x' || ch == 'y' || ch == 'z';

        private bool IsVector()
        {
            int tempPos = pos;
            int length = 1;

            while (tempPos < expr.Length && (expr[tempPos] == '0' || expr[tempPos] == '1'))
            {
                tempPos++;
                length++;
            }
            return length >= 2;
        }

        private string ReadVector()
        {
            string result = currentChar.ToString();
            NextChar();

            while (currentChar == '0' || currentChar == '1')
            {
                result += currentChar;
                NextChar();
            }

            if (!IsPowerOfTwo(result.Length))
                ThrowError("Вектор должен быть длины степени два (1, 2, 4, 8...)");

            return result;
        }

        private bool IsPowerOfTwo(int n) => n > 0 && (n & (n - 1)) == 0;
    }
}

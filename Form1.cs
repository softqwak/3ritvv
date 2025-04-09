using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace _3ritvv
{
    public partial class Form1 : Form
    {
        bool isGoodParse = false;
        List<string> errorList = new List<string>() { };

        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void tbxInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;

            // Разрешаем только цифры, буквы, +, *, (, ), пробелы и управляющие символы
            if (!char.IsControl(ch) && !char.IsDigit(ch) && !char.IsLetter(ch) && ch != '+' && ch != '*' && ch != '(' && ch != ')' && ch != ' ')
            {
                e.Handled = true; // Если символ не разрешён, игнорируем его
            }
        }

        private void tbxInput_TextChanged(object sender, EventArgs e)
        {
            string input = tbxInput.Text;
            var tokens = Tokenize(input);
            var parsed = Parse(tokens);
            double result = Eval(parsed);
            tbxOutput.Text = result.ToString();

            if (isGoodParse)
            {
                errorList.Clear();
                tbxError.Text = "";
            }
            else
            { 
                // Переворачиваем список ошибок
                errorList.Reverse();

                // Выводим ошибки в TextBox в обратном порядке
                tbxError.Text = string.Join(Environment.NewLine, errorList);
                tbxOutput.Text = "";
            }
            isGoodParse = true;

        }

        private List<string> Tokenize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            input = input.Replace("(", " ( ").Replace(")", " ) ");
            return input
                .Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        private Expr? Parse(List<string> tokens)
        {
            if (tokens.Count == 0)
                return null;

            try
            {
                return ParseExpr(tokens);
            }
            catch (Exception ex)
            {
                isGoodParse = false;
                return null;
            }
        }

        private Expr ParseExpr(List<string> tokens)
        {
            if (tokens.Count == 0)
            {
                isGoodParse = false;
                errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Ожидался токен, но достигнут конец ввода.").Message);
            }

            string token = tokens[0];
            tokens.RemoveAt(0);

            if (token == "(")
            {
                var elements = new List<Expr>();

                while (tokens.Count > 0 && tokens[0] != ")")
                {
                    elements.Add(ParseExpr(tokens));
                }

                if (tokens.Count == 0)
                {
                    isGoodParse = false;
                    errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Пропущена закрывающая скобка \')\'.").Message);
                }

                tokens.RemoveAt(0); // Удаляем ')'
                return new ListExpr(elements);
            }
            else if (token == ")")
            {
                isGoodParse = false;
                errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Неожиданная закрывающая скобка \')\'.").Message);
            }
            else
            {
                return Atom(token);
            }
            return null;
        }

        private Expr Atom(string token)
        {
            if (double.TryParse(token, out double number))
                return new Number(number);

            return new Symbol(token);
        }

        private double Eval(Expr expr)
        {
            if (expr is Number num)
                return num.Value;

            if (expr is ListExpr list)
            {
                if (list.Elements.Count == 0)
                {
                    isGoodParse = false;
                    errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Пустое выражение.").Message);
                    return 0;
                }

                if (list.Elements[0] is Symbol op)
                {
                    var args = list.Elements.Skip(1).Select(Eval).ToList();

                    if (op.Name == "+")
                    {
                        return args.Sum();
                    }
                    else if (op.Name == "*")
                    {
                        return args.Aggregate(1.0, (acc, x) => acc * x);
                    }
                    else
                    {
                        isGoodParse = false;
                        errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + $"Неизвестный оператор {op.Name}.");
                        return 0;  // Или выбросить исключение
                    }
                }
                else
                {
                    isGoodParse = false;
                    errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Первый символ в списке не оператор.").Message);
                }
            }
            isGoodParse = false;
            errorList.Add(DateTime.Now.ToString("HH:mm:ss") + $" <{tbxInput.Text}> " + new Exception("Неизвестное выражение.").Message);
            return 0;
        }
    }
    abstract class Expr { }

    class Number : Expr
    {
        public double Value;
        public Number(double value) => Value = value;
    }

    class Symbol : Expr
    {
        public string Name;
        public Symbol(string name) => Name = name;
    }

    class ListExpr : Expr
    {
        public List<Expr> Elements;
        public ListExpr(List<Expr> elements) => Elements = elements;
    }
}

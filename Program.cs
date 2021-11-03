using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;


Console.WriteLine("Empezamos");
//Inicializamos los elementos del programa
var repositorio = new RepositorioCSV();
var sistema = new Sistema(repositorio);
var vista = new Vista();
var controlador = new Controlador(sistema, vista);
controlador.Run();
Console.WriteLine("Fin");

//Clase que se dedica a enseñar al usuario el programa y recoger los datos que nos da el usuario
public class Vista
{
    //Metodo para ver lo que nos mete el usuario es un entero 
    public int obtenerEntero(string prompt)
    {
        //Creamos las variables que vamos a usar en el metodo
        int entero = int.MinValue;
        string input = "";
        bool entradaIncorrecta = true;
        while (entradaIncorrecta)
        {
            try
            {
                //Escribimos el prompt sin espacios al principio del String y tampoco en el final
                Console.Write($"   {prompt.Trim()}: ");
                //Leemos los que nos pone el usuario
                input = Console.ReadLine();
                //Si los que nos han puesto no es igual a fin, guarda el input en el entero y cambia el booleano a true.
                if (input != "fin")
                {
                    entero = int.Parse(input);
                    entradaIncorrecta = false;
                }
                else
                {
                    entero = int.MinValue;
                    entradaIncorrecta = false;
                }
            }
            catch (FormatException)
            {
                ;
            }
        }
        //Devuelve un entero dependiendo de lo que nos hayan puesto.
        return entero;
    }
    //Despliega un menu en el que puedes pasar parametros al programa
    public int obtenerOpcion(string titulo, Object[] opciones, string prompt)
    {
        Console.WriteLine($"   === {titulo} ===");
        Console.WriteLine();
        //Para enseñar todas la opciones de la lista de opciones que le pasan
        for (int i = 0; i < opciones.Length; i++)
        {
            Console.WriteLine($"   {i + 1:##}.- {opciones[i]}");
        }
        Console.WriteLine();
        return obtenerEntero(prompt);
    }
}

//Clase que comunica el sistema y la vista
public class Controlador
{
    //El menu que de las opciones que hace el programa
    string[] menu = new[]{
       "Obtener la media de las notas",
       "Obtener la mejor nota"
       };

    private Sistema sistema;
    private Vista vista;

    //Constructor que pide un sistema y una vista
    public Controlador(Sistema sistema, Vista vista)
    {
        this.sistema = sistema;
        this.vista = vista;
    }

    //Lo que hace el programa al arrancar
    public void Run()
    {
        while (true)
        {
            Console.Clear();
            //Le pasa parametros a la vista para que el usuario lo lea y meta una opcion, y la variable opcion lo recoge
            var opcion = vista.obtenerOpcion("Menu de Opciones", menu, "Seleciona una opción");
            //Dependiendo de lo que nos hayan puesto, el programa hará una cosa u otra
            switch (opcion)
            {
                case 1:
                    obtenerLaMedia();
                    break;
                case 2:
                    obtenerMasAlta();
                    break;
                case int.MinValue:
                    // Salimos 
                    return;
            }
            Console.WriteLine("\n\nPulsa Return para continuar");
            Console.ReadLine();
        }
    }
    //Llama al sistema para que le haga un calculo de la media
    public void obtenerLaMedia()
    {
        Console.WriteLine($"La media de la notas es: {sistema.CalculoDeLaMedia():0.00}");
    }
    //Llama al sistema para que le devuelva cual es la nota más alta
    public void obtenerMasAlta(){
        Console.WriteLine($"La nota mas alta es: {sistema.NotaMasAlta()}" );
    }


}

//Clase para almacenar los datos
public class Calificacion
{
    public string Nombre;
    public decimal Nota;

    //Modificamos el ToString con esta clase para que imprima el nombre y la nota en un string
    public override string ToString() => $"({Nombre}, {Nota})";

    //Pasamos los datos que nos dan de un string a los datos que componen una calificacion
    internal static Calificacion ParseRow(string row)
    {
        //Para usar el sistema de numeros de EEUU
        NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;

        //Dividimos el parametro que nos han mandado que esten separados entre comas un una lista.
        var columns = row.Split(',');
        //Devuelve una nueva calificacion con los datos ya metidos dentro de ella
        return new Calificacion()
        {
            Nombre = columns[0],
            Nota = decimal.Parse(columns[1], nfi)
        };

    }
}
//Clase que se encarga de las operaciones
public class Sistema
{
    IRepositorio Repositorio;

    List<Calificacion> Notas;

    //Constructor que necesita de un repositorio para saber de donde tiene que coger los datos
    public Sistema(IRepositorio repositorio)
    {
        Repositorio = repositorio;
        Repositorio.Inicializar();
        Notas = Repositorio.CargarCalificaciones();
    }

    //A esta funcion le pasas un array de decimales y los suma
    private decimal CalculoDeLaSuma(decimal[] datos) => datos.Sum();

    //Esta funcion coge la variable notas que pilla una vez inicializas la clase Sistema
    public decimal CalculoDeLaMedia()
    {
        //Crea una array de notas de la calificacion.Nota que habia en Notas
        var notas = Notas.Select(calificacion => calificacion.Nota).ToArray();
        //Devuelve la suma de las notas divida por el numero de Notas totales que hay, es devuelve la media
        return CalculoDeLaSuma(notas) / Notas.Count;
    }

    //Devuelve la nota mas alta 
    public decimal NotaMasAlta(){
        return Notas.Max(calificacion => calificacion.Nota);
    }
}

//Creamos una interface de repositorio, como una especie de plantilla que no podemos inicializar
public interface IRepositorio
{
    void Inicializar();
    List<Calificacion> CargarCalificaciones();

}

//Creamos una clase de repositorio que herede de la interface iRepositorio
public class RepositorioCSV : IRepositorio
{
    string datafile;
    //Al inicializarlo le ponemos que fichero tiene que leer
    void IRepositorio.Inicializar()
    {
        this.datafile = "notas.txt";
    }
    //funcion para cargar las calificaciones que devuelve una lista de calificaciones
    List<Calificacion> IRepositorio.CargarCalificaciones()
    {
        //Lee todas las lineas del archivo datafile
        return File.ReadAllLines(datafile)
            //Skipea la primera linea, por que contiene el orden de los datos
            .Skip(1)
            //Si la linea tiene una longitud de 0, la ignora
            .Where(row => row.Length > 0)
            //Las pasa a la lista usando el metodo ParseRow de la clase calificacion
            .Select(Calificacion.ParseRow).ToList();
    }


}
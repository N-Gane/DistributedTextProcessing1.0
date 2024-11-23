using MPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        // Инициализация MPI
        using (new MPI.Environment(ref args))
        {
            Intracommunicator comm = Communicator.world;
            int rank = comm.Rank;
            int size = comm.Size;

            // Узел-координатор читает файл
            List<Point3D> points = null;
            if (rank == 0)
            {
                string filePath = args[0];
                points = ReadPointsFromFile(filePath);
            }

            // Распределяем данные между узлами (требуется ref)
            comm.Broadcast(ref points, 0);

            // Вычисляем площади параллелограммов
            var areas = CalculateParallelograms(points, rank, size);

            // Собираем результаты на главном узле
            var allAreas = comm.Gather(areas, 0);

            if (rank == 0)
            {
                // Сохраняем результат в файл
                var resultFile = "result.txt";
                File.WriteAllLines(resultFile, allAreas.SelectMany(x => x).Select(area => area.ToString()));
                Console.WriteLine("Результаты сохранены в " + resultFile);
            }
        }
    }

    static List<Point3D> ReadPointsFromFile(string filePath)
    {
        return File.ReadLines(filePath)
                   .Select(line => line.Split(' '))
                   .Select(parts => new Point3D(
                       double.Parse(parts[0]),
                       double.Parse(parts[1]),
                       double.Parse(parts[2])))
                   .ToList();
    }

    static List<double> CalculateParallelograms(List<Point3D> points, int rank, int size)
    {
        var areas = new List<double>();
        int start = rank * (points.Count / size);
        int end = (rank == size - 1) ? points.Count : start + (points.Count / size);

        for (int i = start; i < end; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                for (int k = j + 1; k < points.Count; k++)
                {
                    var area = CalculateParallelogramArea(points[i], points[j], points[k]);
                    areas.Add(area);
                }
            }
        }
        return areas;
    }

    static double CalculateParallelogramArea(Point3D p1, Point3D p2, Point3D p3)
    {
        var vector1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        var vector2 = new Point3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

        var crossProduct = new Point3D(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X
        );

        return Math.Sqrt(crossProduct.X * crossProduct.X +
                         crossProduct.Y * crossProduct.Y +
                         crossProduct.Z * crossProduct.Z);
    }
}

class Point3D
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

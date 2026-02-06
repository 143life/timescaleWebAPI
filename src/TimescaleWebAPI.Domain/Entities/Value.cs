using TimescaleWebAPI.Domain.Exceptions;

namespace TimescaleWebAPI.Domain.Entities;

public class Value
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public DateTime Date { get; private set; }
    public double ExecutionTime { get; private set; }
    public double ValueMetric { get; private set; } // Переименовали чтобы не конфликтовать с именем класса
    public DateTime CreatedAt { get; protected set; }

    // Приватный конструктор для EF Core
    private Value() { }

    public Value(string fileName, DateTime date, double executionTime, double valueMetric)
    {
        Id = Guid.NewGuid();
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Date = date;
        ExecutionTime = executionTime;
        ValueMetric = valueMetric;
        CreatedAt = DateTime.UtcNow;
        
        Validate();
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(FileName))
            throw new DomainException("FileName cannot be empty");
            
        if (Date < new DateTime(2000, 1, 1))
            throw new DomainException("Date cannot be earlier than 2000-01-01");
            
        if (Date > DateTime.UtcNow)
            throw new DomainException("Date cannot be in the future");
            
        if (ExecutionTime < 0)
            throw new DomainException("ExecutionTime cannot be negative");
            
        if (ValueMetric < 0)
            throw new DomainException("Value cannot be negative");
    }

    // Бизнес-методы, если нужны
    public void UpdateFileName(string newFileName)
    {
        FileName = newFileName ?? throw new ArgumentNullException(nameof(newFileName));
        Validate();
    }
}
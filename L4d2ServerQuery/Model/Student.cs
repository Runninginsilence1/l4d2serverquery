namespace L4d2ServerQuery.Model;

public class Student
{
    public int ID { get; set; } // EFCore 会自动将 ID 或者 具有 ClassNameID 的字段识别成主键
    public string LastName { get; set; }
    public string FirstMidName { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } // 多对多的连接表, 可以使用任意实现了 ICollection<T> 的集合类型, 如果直接指定 ICollection<Enrollment>, 则底层的类型为 HashSet<Enrollment> 
    
}
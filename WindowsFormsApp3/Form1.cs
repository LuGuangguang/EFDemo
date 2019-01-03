using EFCommon.SqlHelp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Repository repository = new Repository();
        private void button1_Click(object sender, EventArgs e)
        {
            TestEntities context = new TestEntities();
            //Student user = new Student() { Id = 10005 };
            ////将要删除的对象附加到EF容器中
            //context.Students.Attach(user);
            ////Remove()起到了标记当前对象为删除状态，可以删除
            //context.Students.Remove(user);
            //context.SaveChanges();
            //Console.WriteLine("删除成功");


            //1.获得要更新后的数据，在mvc中的Action方法，可直接获得 更新后的对象
            Student u = new Student() { Id = 10008, Name = "kim" };
            //2.标识为修改
            context.Entry<Student>(u).State = EntityState.Modified;
            //3.保存到数据库
            context.SaveChanges();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TestEntities context = new TestEntities();
            var a3 = context.Students.ToList();


          

       
           

        }

        //public static string GetMemberName<T>(Expression<Func<T>> memberExpression)
        //{
        //    MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
        //    return expressionBody.Member.Name;
        //}
        public static string GetMemberName<T>(string memberExpression)
        {
            Expression<Func<string>> expression = () => "";
            MemberExpression expressionBody = (MemberExpression)expression.Body;
            return expressionBody.Member.Name;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //增加
            Student student = new Student() { Id = 10008, Name = "zz" };
            int aa = repository.Add(student);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //查询
            
            var a2 = repository.GetAll<Student>();
            var a = repository.GetAllQuery<Student>(item => item.Id == 10006 || item.Id == 10007 || item.Id == 10008).OrderBy(item => item.Age).ToList();
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            //删除
            int b = repository.Delete<Student>(item => item.Age == 180);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //修改
            int bbb = repository.Update<Student>(new Student() { Name = "dddd", Age = 180 }, item => item.Age == 1, new string[] { Repository.GetMemberName(() => new Student().Name), Repository.GetMemberName(() => new Student().Age) });
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}

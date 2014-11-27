class P{static void Main(string[] a)
{int x,y,z,d,i,j,l,m=0;
for(i=0;i<a.Length;x=a[i++].Length,m=x>m?x:m);
for(i=0;i<a.Length;i++)
{
var s=a[i].Split();z=s.Length;
var o="";
if (z<2)o=a[i];
else
{
for(j=0,l=0;j<z;l+=s[j++].Length);
d=m-l;
x=d/(z-1);
y=d%(z-1);
for (j=0;j<z;o+=s[j]+new string(' ',x)+(y>j++?" ":""));
}System.Console.WriteLine(o);}}}

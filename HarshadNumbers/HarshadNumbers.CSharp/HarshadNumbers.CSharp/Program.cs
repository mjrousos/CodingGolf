class P{static int Main(string[]a){int i=0,t,s,c=0;for(;i+++""!=a[0];c+=i%s<1?1:0)for(s=0,t=i;t>0;t/=10)s+=t%10;return c;}}
class P{static int Main(string[]a){int i=0,t,s=0,c=0;for(;i+++""!=a[0];c+=i%++s<1?1:0)for(t=10;i%t<1;t*=10)s-=9;return c;}}
class P{static int Main(string[]a){int i=0,t,s=0,c=0;for(;i+++""!=a[0];c+=i%++s<1?1:0)for(t=i;t%10<1;t/=10)s-=9;return c;}}

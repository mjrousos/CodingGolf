foreach(1..$ARGV[0]){$t=$_;while($t%10<1){$t/=10;$s-=9}if($_%++$s<1){$c++}}exit $c
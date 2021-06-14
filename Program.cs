using System;

namespace x_www_form_urlencoded_To_Json
{
    class Program
    {
        static void Main(string[] args)
        {
            string urlDecodedString = "type=subscribe&fired_at=2021-06-11+08%3A54%3A35&data%5Bid%5D=b9a27e5b65&data%5Bemail%5D=anna%40mailinator.se&data%5Bemail_type%5D=html&data%5Bip_opt%5D=89.233.242.116&data%5Bweb_id%5D=513850286&data%5Bmerges%5D%5BEMAIL%5D=anna%40mailinator.se&data%5Bmerges%5D%5BFNAME%5D=Anna&data%5Bmerges%5D%5BLNAME%5D=Gebhard&data%5Bmerges%5D%5BADDRESS%5D=&data%5Bmerges%5D%5BPHONE%5D=&data%5Bmerges%5D%5BBIRTHDAY%5D=03%2F12&data%5Bmerges%5D%5BINTERESTS%5D=&data%5Bmerges%5D%5BGROUPINGS%5D%5B0%5D%5Bid%5D=78830&data%5Bmerges%5D%5BGROUPINGS%5D%5B0%5D%5Bunique_id%5D=8c7db2a675&data%5Bmerges%5D%5BGROUPINGS%5D%5B0%5D%5Bname%5D=Donerare&data%5Bmerges%5D%5BGROUPINGS%5D%5B0%5D%5Bgroups%5D=&data%5Bmerges%5D%5BGROUPINGS%5D%5B1%5D%5Bid%5D=78834&data%5Bmerges%5D%5BGROUPINGS%5D%5B1%5D%5Bunique_id%5D=c481c42f26&data%5Bmerges%5D%5BGROUPINGS%5D%5B1%5D%5Bname%5D=Flicka&data%5Bmerges%5D%5BGROUPINGS%5D%5B1%5D%5Bgroups%5D=&data%5Blist_id%5D=cba9436ff6";
            string jsonString = UrlEncodedToJsonCoverter.UrlEncodedJsonifier(urlDecodedString);
            Console.WriteLine(jsonString);
        }
    }
}

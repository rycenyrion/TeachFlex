using System;
using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public static class AutomatedRemarksCatalog
    {
        private static readonly IReadOnlyDictionary<
            int,
            IReadOnlyDictionary<AutomatedRemarkLevel, string[]>>
            RemarksByGrade =
                new Dictionary<
                    int,
                    IReadOnlyDictionary<AutomatedRemarkLevel, string[]>>
                {
                    [1] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging husay sa pagkatuto at palaging nagsisikap sa mga gawain.",
                            "Napakahusay niyang natututuhan ang mga aralin at naipapakita ito sa kanyang mga gawain.",
                            "Palaging aktibo, masipag, at masayang nakikilahok sa mga gawain sa klase.",
                            "Nagpapakita ng mabuting asal, disiplina, at respeto sa guro at mga kaklase.",
                            "Mabilis matuto at mahusay gumamit ng mga natutunang kasanayan.",
                            "Palaging handa sa klase at nagbibigay ng pinakamahusay na pagsisikap sa bawat gawain.",
                            "Nakakamit ang mga layunin sa pagkatuto nang higit sa inaasahan.",
                            "Mahusay sumunod sa mga panuto at maayos gumawa ng mga aktibidad.",
                            "Nagpapakita ng pagiging malikhain at masigasig sa mga gawain.",
                            "Isang huwarang mag-aaral na nagpapakita ng positibong pag-uugali.",
                            "Mahusay makipagtulungan sa mga kaklase at guro.",
                            "Patuloy na nagpapakita ng mataas na interes sa pag-aaral.",
                            "Naipapakita ang kumpiyansa sa sarili sa pagsagot at pagsasagawa ng gawain.",
                            "Ipinagmamalaki ang kanyang sipag, tiyaga, at magandang pag-uugali.",
                            "Ipagpatuloy ang mahusay na pagganap at pagiging inspirasyon sa klase."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagsisikap sa pag-aaral.",
                            "Naipapakita ang magandang pag-unawa sa mga pangunahing aralin.",
                            "Masipag at responsable sa paggawa ng mga gawain sa paaralan.",
                            "Aktibong nakikilahok sa mga talakayan at aktibidad.",
                            "Nagpapakita ng magandang asal at pakikipagkapwa sa klase.",
                            "Maayos na sumusunod sa mga panuto ng guro.",
                            "Patuloy na umuunlad sa pagbasa, pagsulat, at pagbibilang.",
                            "May positibong saloobin at interes sa pag-aaral.",
                            "Nakakamit ang karamihan sa mga kasanayang inaasahan sa baitang.",
                            "Maayos na natatapos ang mga gawain sa itinakdang oras.",
                            "Nagpapakita ng pagiging masipag at matiyaga sa pagkatuto.",
                            "Marunong makinig at sumunod sa mga alituntunin ng klase.",
                            "Patuloy na pinauunlad ang sariling kakayahan.",
                            "May magandang pakikitungo sa guro at mga kamag-aral.",
                            "Ipagpatuloy ang pagsisikap upang lalo pang umunlad."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga aralin at kasanayan.",
                            "Patuloy na nagsisikap upang matutuhan ang mga bagong aralin.",
                            "Nakikilahok sa mga gawain sa klase sa tulong ng guro.",
                            "Gumagawa ng mga aktibidad ngunit nangangailangan pa ng kaunting gabay.",
                            "May kakayahang matuto at umunlad sa pamamagitan ng pagsasanay.",
                            "Patuloy na nagpapakita ng pag-unlad sa pagbasa, pagsulat, at pagbibilang.",
                            "Nakikinig sa mga talakayan at sumusunod sa mga panuto.",
                            "Nagpapakita ng magandang asal sa loob ng silid-aralan.",
                            "Kailangan lamang ng patuloy na pagsasanay upang maging mas mahusay.",
                            "May interes sa pag-aaral at handang matuto ng mga bagong bagay.",
                            "Unti-unting nagkakaroon ng tiwala sa sarili sa paggawa ng gawain.",
                            "Patuloy na pinauunlad ang kanyang mga kasanayan.",
                            "Nakakamit ang mga pangunahing layunin sa pagkatuto.",
                            "Nangangailangan lamang ng gabay upang higit pang umunlad.",
                            "Ipagpatuloy ang pagsisikap at pagiging masipag sa pag-aaral."
                        },
                        new[]
                        {
                            "Nangangailangan pa ng dagdag na pagsasanay upang mapaunlad ang kanyang mga kasanayan.",
                            "Kailangan ng higit na paggabay upang maunawaan ang mga aralin.",
                            "Hikayatin siyang maging mas aktibo sa mga gawain sa klase.",
                            "Nangangailangan ng karagdagang pagsisikap sa pagbasa, pagsulat, at pagbibilang.",
                            "Kailangan pang sanayin sa pagsunod sa mga panuto.",
                            "May kakayahang umunlad sa pamamagitan ng patuloy na pagsasanay.",
                            "Nangangailangan ng tulong upang matapos nang maayos ang mga gawain.",
                            "Kailangan ng dagdag na suporta upang magkaroon ng tiwala sa sarili.",
                            "Hikayatin ang regular na pagsasanay sa bahay.",
                            "Patuloy na gabayan upang mapaunlad ang kanyang pagkatuto.",
                            "Kailangan pang pagbutihin ang pakikinig at pagtutok sa klase.",
                            "Nangangailangan ng higit na pagsasanay upang makamit ang inaasahang kasanayan.",
                            "May ipinapakitang pag-unlad ngunit kailangan pa ng karagdagang suporta.",
                            "Dapat hikayatin na maging mas masipag sa paggawa ng mga gawain.",
                            "Sa tulong ng paggabay at pagsisikap, makakamit niya ang pag-unlad."
                        },
                        new[]
                        {
                            "Nangangailangan ng mas maraming gabay upang mapaunlad ang mga pangunahing kasanayan.",
                            "Kailangan ng karagdagang pagsasanay sa pagbasa, pagsulat, at pagbibilang.",
                            "Nangangailangan ng tulong upang mas maunawaan ang mga aralin.",
                            "Hikayatin ang regular na pagsasanay at pag-aaral sa bahay.",
                            "Kailangan pang paunlarin ang pagiging masipag at responsable sa gawain.",
                            "Nangangailangan ng patuloy na paggabay mula sa guro at magulang.",
                            "Kailangan ng mas aktibong pakikilahok sa mga gawain sa klase.",
                            "Hikayatin na magkaroon ng positibong saloobin sa pag-aaral.",
                            "Kailangan pang pagbutihin ang pagsunod sa mga panuto.",
                            "Nangangailangan ng dagdag na pagsasanay upang malinang ang kanyang kakayahan.",
                            "Kailangan ng tulong upang mapanatili ang pokus sa mga gawain.",
                            "Patuloy na tutulungan upang mapaunlad ang kanyang kaalaman at kasanayan.",
                            "Dapat bigyan ng higit na oras at gabay sa pagsasanay.",
                            "Kailangan ng suporta upang makamit ang mga layunin sa pagkatuto.",
                            "Sa patuloy na pagsisikap at paggabay, makakamit niya ang inaasahang pag-unlad."
                        }
                    ),
                    [2] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging husay sa lahat ng gawain at patuloy na nagsisikap na maging mahusay.",
                            "Napakahusay niyang natutunan ang mga aralin at naipapakita ito sa kanyang mga gawain at pagsusulit.",
                            "Palaging aktibo sa klase at nagbibigay ng makabuluhang kontribusyon sa mga talakayan.",
                            "Nagpapakita ng mahusay na pag-uugali, disiplina, at pagiging responsable bilang mag-aaral.",
                            "Patuloy na nagpapakita ng mataas na antas ng kasipagan at dedikasyon sa pag-aaral.",
                            "Napakahusay sa pagsunod sa mga panuto at pagkumpleto ng mga gawain nang may kalidad.",
                            "Malaki ang ipinakitang pag-unlad at kahusayan sa iba’t ibang asignatura.",
                            "May mahusay na kakayahan sa pag-unawa at paggamit ng mga natutunang konsepto.",
                            "Palaging handa, masipag, at positibo sa bawat gawain sa paaralan.",
                            "Ipinagmamalaki ang kanyang mahusay na pagganap at pagiging huwarang mag-aaral.",
                            "Nagpapakita ng pagiging malikhain at mahusay na kasanayan sa paglutas ng mga gawain.",
                            "Patuloy na nagbibigay ng inspirasyon sa kanyang mga kaklase sa pamamagitan ng magandang halimbawa.",
                            "Nakakamit ang mga layunin sa pagkatuto nang higit sa inaasahan.",
                            "May mataas na tiwala sa sarili at mahusay makipagtulungan sa iba.",
                            "Ipagpatuloy ang ganitong kahanga-hangang pagganap at positibong pananaw sa pag-aaral."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagpapakita ng pagsisikap sa pag-aaral.",
                            "Naipapakita niya ang mabuting pag-unawa sa mga aralin at gawain.",
                            "Masipag at responsable sa pagtupad ng mga tungkulin bilang mag-aaral.",
                            "Aktibo sa klase at nakikilahok sa mga talakayan at gawain.",
                            "Patuloy na nagpapakita ng magandang asal at pakikipagkapwa sa mga kaklase.",
                            "Nakakamit ang karamihan sa mga inaasahang kasanayan sa bawat aralin.",
                            "Maayos niyang natatapos ang mga gawain sa itinakdang oras.",
                            "Nagpapakita ng interes at positibong saloobin sa pagkatuto.",
                            "May kakayahang gamitin ang kanyang natutunan sa iba’t ibang sitwasyon.",
                            "Patuloy na nagsisikap upang mapabuti pa ang kanyang pagganap.",
                            "Mahusay makinig at sumusunod sa mga panuto ng guro.",
                            "Ipinapakita ang pagiging responsable at masipag sa kanyang mga gawain.",
                            "Maganda ang kanyang pakikilahok sa mga aktibidad sa klase.",
                            "May magandang pag-unlad sa akademiko at pag-uugali.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang higit pang tagumpay."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga aralin at patuloy na umuunlad.",
                            "Ginagawa ang mga gawain ngunit kailangan pa ng kaunting paggabay upang maging mas mahusay.",
                            "Nakikilahok sa klase at nagsisikap na matutuhan ang mga bagong aralin.",
                            "Patuloy na nagpapakita ng pagsisikap sa pagkumpleto ng mga gawain.",
                            "May kakayahan siyang matuto at mapaunlad pa ang kanyang mga kasanayan.",
                            "Nakakamit ang mga pangunahing layunin sa pagkatuto.",
                            "Kailangan lamang ng patuloy na pagsasanay upang mas mapabuti ang pagganap.",
                            "Nagpapakita ng magandang asal at pakikipagtulungan sa klase.",
                            "Nakikinig sa mga panuto at nagsisikap na maisagawa nang maayos ang mga gawain.",
                            "Patuloy na nangangailangan ng pagsasanay upang mapalawak ang kaalaman.",
                            "May positibong pananaw sa pag-aaral at handang matuto.",
                            "Gumagawa ng mga gawain ngunit kailangan pa ng dagdag na pagsisikap.",
                            "Nakikita ang unti-unting pag-unlad sa kanyang mga kasanayan.",
                            "Patuloy na gabayan upang lalo pang mapaunlad ang kakayahan.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang mas mataas na antas ng pagkatuto."
                        },
                        new[]
                        {
                            "Kailangan pa ng higit na pagsasanay upang mapabuti ang kanyang pagganap sa klase.",
                            "Nangangailangan ng dagdag na paggabay sa pag-unawa ng mga aralin.",
                            "Kailangan pang pagbutihin ang pagsunod sa mga panuto at pagkumpleto ng gawain.",
                            "Patuloy na hikayatin upang maging mas aktibo sa mga gawain sa klase.",
                            "Nangangailangan ng mas regular na pagsasanay upang mapaunlad ang kanyang kasanayan.",
                            "May kakayahang umunlad kung maglalaan ng higit na pagsisikap sa pag-aaral.",
                            "Kailangan pa ng gabay upang maging mas responsable sa kanyang mga gawain.",
                            "Dapat dagdagan ang oras sa pag-aaral at pagsasanay sa bahay.",
                            "Nangangailangan ng tulong upang mas maunawaan ang ilang mahahalagang konsepto.",
                            "Hikayatin siyang makilahok nang mas aktibo sa mga talakayan.",
                            "May mga natutunang kasanayan ngunit kailangan pa ng pagpapaunlad.",
                            "Kailangan ng patuloy na suporta mula sa guro at magulang.",
                            "Dapat pagbutihin ang pagiging masipag at masinop sa paggawa ng mga gawain.",
                            "Patuloy na gabayan upang magkaroon ng higit na kumpiyansa sa sarili.",
                            "Kailangan ng mas malaking pagsisikap upang makamit ang inaasahang pagkatuto."
                        },
                        new[]
                        {
                            "Nangangailangan ng higit na gabay at suporta upang mapaunlad ang kanyang pagkatuto.",
                            "Kailangan pang pagtuunan ng pansin ang mga pangunahing kasanayan sa pag-aaral.",
                            "Hikayatin ang regular na pagsasanay at paggawa ng mga takdang-aralin.",
                            "Nangangailangan ng karagdagang tulong upang maunawaan ang mga aralin.",
                            "Kailangan pang paunlarin ang pagiging responsable sa mga gawain sa paaralan.",
                            "Dapat bigyan ng mas maraming pagkakataon upang magsanay at matuto.",
                            "Nangangailangan ng patuloy na paggabay upang mapaunlad ang kanyang kakayahan.",
                            "Kailangan ng mas aktibong pakikilahok sa mga gawain sa klase.",
                            "Hikayatin na magkaroon ng mas positibong saloobin sa pag-aaral.",
                            "Kailangan ng regular na suporta mula sa guro at magulang upang umunlad.",
                            "Nangangailangan ng dagdag na pagsasanay sa mga pangunahing kasanayan.",
                            "Dapat magsikap nang higit upang makamit ang mga layunin sa pagkatuto.",
                            "Kailangan pang paunlarin ang konsentrasyon at pagtutok sa klase.",
                            "Patuloy na tutulungan upang magkaroon ng pag-unlad sa akademiko at pag-uugali.",
                            "Sa tulong ng patuloy na paggabay at pagsisikap, makakamit niya ang inaasahang pag-unlad."
                        }
                    ),
                    [3] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging husay sa pag-aaral at patuloy na nakakamit ang mga layunin sa pagkatuto.",
                            "Napakahusay ng kanyang pagganap sa lahat ng asignatura at gawain sa klase.",
                            "Palaging masipag, responsable, at aktibong nakikilahok sa mga talakayan.",
                            "Nagpapakita ng mataas na antas ng disiplina, pagiging malikhain, at pagtitiyaga.",
                            "Mahusay niyang naipapakita ang kanyang kaalaman at kakayahan sa iba’t ibang gawain.",
                            "Palaging handa sa klase at nagbibigay ng magandang halimbawa sa kanyang mga kaklase.",
                            "Ipinapakita ang mahusay na paggamit ng mga natutunang kasanayan sa pang-araw-araw na gawain.",
                            "Nakakamit ang mga inaasahang kasanayan nang higit pa sa pamantayan.",
                            "Patuloy na nagpapakita ng kahanga-hangang pag-uugali at positibong pananaw sa pag-aaral.",
                            "Mahusay makipagtulungan at nagpapakita ng pamumuno sa mga gawaing pangklase.",
                            "Ipinagmamalaki ang kanyang pagiging masipag at dedikadong mag-aaral.",
                            "Nagpapakita ng mataas na kalidad ng paggawa sa mga takdang-aralin at proyekto.",
                            "Patuloy na nagpapakita ng kahusayan at pagiging huwaran sa klase.",
                            "May malaking kakayahan at patuloy na pinauunlad ang sariling talento.",
                            "Ipagpatuloy ang mahusay na pagganap at positibong saloobin sa pag-aaral."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagsisikap upang mapaunlad ang sarili.",
                            "Naipapakita ang magandang pag-unawa sa mga aralin at konseptong tinatalakay.",
                            "Masipag at responsable sa pagsasagawa ng mga gawain sa paaralan.",
                            "Aktibong nakikilahok sa mga talakayan at pangkatang gawain.",
                            "Nagpapakita ng magandang asal at paggalang sa guro at mga kaklase.",
                            "Nakakamit ang karamihan sa mga layunin ng pagkatuto.",
                            "Maayos na natatapos ang mga gawain at sumusunod sa mga panuto.",
                            "Patuloy na nagpapakita ng interes at kasipagan sa pag-aaral.",
                            "May kakayahang gamitin ang natutuhan sa iba’t ibang gawain.",
                            "Nagpapakita ng positibong pananaw at pagiging responsable bilang mag-aaral.",
                            "Mahusay makibahagi sa mga aktibidad at proyekto ng klase.",
                            "Patuloy na umuunlad sa akademiko at personal na pag-uugali.",
                            "Nakikita ang kanyang pagsisikap upang maging mas mahusay pa.",
                            "May magandang kakayahan at patuloy itong pinauunlad.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang higit pang tagumpay."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga pangunahing aralin at kasanayan.",
                            "Patuloy na nagsisikap upang mapabuti ang kanyang pagganap sa klase.",
                            "Nakikilahok sa mga gawain at sumusunod sa mga panuto ng guro.",
                            "May kakayahang matuto at mapaunlad pa ang kanyang mga kasanayan.",
                            "Ginagawa ang mga takdang-aralin at aktibidad nang may gabay.",
                            "Patuloy na nagpapakita ng pag-unlad sa iba’t ibang larangan.",
                            "Kailangan lamang ng karagdagang pagsasanay upang maging mas mahusay.",
                            "Nagpapakita ng magandang asal at pakikipagtulungan sa klase.",
                            "May sapat na kaalaman sa mga paksang tinatalakay.",
                            "Patuloy na pinauunlad ang kanyang kakayahan sa pag-aaral.",
                            "Nakikinig sa mga talakayan at nagsisikap na matuto.",
                            "Gumagawa ng mga gawain ngunit kailangan pa ng kaunting gabay.",
                            "May positibong saloobin sa pag-aaral at pagtanggap ng mga hamon.",
                            "Nakakamit ang karamihan sa inaasahang kasanayan sa baitang.",
                            "Ipagpatuloy ang pagsisikap upang higit pang mapaunlad ang kanyang kakayahan."
                        },
                        new[]
                        {
                            "Nangangailangan pa ng dagdag na pagsasanay upang mapaunlad ang kanyang kaalaman at kasanayan.",
                            "Kailangan ng higit na paggabay upang lubos na maunawaan ang mga aralin.",
                            "Hikayatin siyang maging mas aktibo sa mga gawain at talakayan sa klase.",
                            "Nangangailangan ng mas regular na pagsasanay upang mapabuti ang pagganap.",
                            "Kailangan pang pagbutihin ang pagsunod sa mga panuto at paggawa ng gawain.",
                            "May kakayahang umunlad kung patuloy na magsisikap sa pag-aaral.",
                            "Kailangan ng mas maraming pagsasanay sa mga kasanayang hindi pa lubos na natutuhan.",
                            "Patuloy na gabayan upang maging mas responsable sa mga gawain.",
                            "Nangangailangan ng suporta upang magkaroon ng higit na tiwala sa sarili.",
                            "Dapat pagbutihin ang pagiging masipag at masinop sa paggawa ng aktibidad.",
                            "Kailangan ng dagdag na oras sa pag-aaral upang mapaunlad ang kaalaman.",
                            "Patuloy na tutulungan upang makamit ang inaasahang antas ng pagkatuto.",
                            "Nangangailangan ng gabay sa pagpapabuti ng kanyang akademikong pagganap.",
                            "Hikayatin na maging mas determinado at masipag sa pag-aaral.",
                            "Sa patuloy na pagsisikap at paggabay, makakamit niya ang higit na pag-unlad."
                        },
                        new[]
                        {
                            "Nangangailangan ng masusing paggabay upang mapaunlad ang mga pangunahing kasanayan.",
                            "Kailangan ng karagdagang pagsasanay at suporta upang makamit ang mga layunin sa pagkatuto.",
                            "Hikayatin ang regular na pag-aaral at pagsasanay sa bahay.",
                            "Nangangailangan ng tulong upang mas maunawaan ang mahahalagang konsepto sa aralin.",
                            "Kailangan pang paunlarin ang pagiging responsable sa mga gawain sa paaralan.",
                            "Dapat pagtuunan ng pansin ang pagpapabuti ng kanyang pagganap sa klase.",
                            "Nangangailangan ng patuloy na suporta mula sa guro at magulang.",
                            "Kailangan ng higit na pakikilahok sa mga gawain at talakayan.",
                            "Hikayatin na magkaroon ng positibong pananaw at interes sa pag-aaral.",
                            "Nangangailangan ng karagdagang pagsasanay upang mapaunlad ang mga kasanayan.",
                            "Kailangan pang pagbutihin ang pagtutok at pakikinig sa klase.",
                            "Patuloy na tutulungan upang magkaroon ng pag-unlad sa akademiko at asal.",
                            "Dapat magsikap nang higit upang makamit ang mga inaasahang kasanayan.",
                            "Kailangan ng regular na paggabay upang mapaunlad ang kanyang kakayahan.",
                            "Sa pamamagitan ng pagsisikap, suporta, at patuloy na pagsasanay, makakamit niya ang pag-unlad."
                        }
                    ),
                    [4] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging kahusayan sa pag-aaral at palaging nagsisikap na mapanatili ang mataas na kalidad ng kanyang gawain.",
                            "Napakahusay niyang naipapakita ang kaalaman, kasanayan, at positibong saloobin sa lahat ng asignatura.",
                            "Palaging aktibo sa klase at mahusay makilahok sa mga talakayan at pangkatang gawain.",
                            "Nagpapakita ng mataas na antas ng pagiging responsable, masipag, at disiplinadong mag-aaral.",
                            "Nakakamit ang mga layunin sa pagkatuto nang higit pa sa inaasahang pamantayan.",
                            "Mahusay niyang ginagamit ang mga natutunang kasanayan sa paglutas ng mga gawain at suliranin.",
                            "Patuloy na nagpapakita ng pagiging huwaran sa pag-uugali at pagganap sa klase.",
                            "Nagpapakita ng pagiging malikhain at kritikal na pag-iisip sa iba’t ibang gawain.",
                            "Palaging handa, organisado, at determinado sa bawat gawaing pang-akademiko.",
                            "Mahusay makipagtulungan at nagbibigay ng positibong impluwensya sa mga kaklase.",
                            "Ipinapakita ang dedikasyon at pagmamahal sa pag-aaral sa pamamagitan ng kanyang mga gawa.",
                            "Patuloy na nagpapaunlad ng sariling kakayahan at talento.",
                            "Nakapagbibigay ng mahusay na output sa mga proyekto at aktibidad.",
                            "Isang huwarang mag-aaral na nagpapakita ng kahusayan at magandang asal.",
                            "Ipagpatuloy ang kahanga-hangang pagganap at pagiging masipag sa pag-aaral."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagpapakita ng pagsisikap sa pag-aaral.",
                            "Naipapakita niya ang magandang pag-unawa sa mga aralin at kasanayang tinatalakay.",
                            "Responsable sa paggawa ng mga gawain at pagsunod sa mga panuto.",
                            "Aktibong nakikilahok sa mga gawain at talakayan sa klase.",
                            "Nagpapakita ng magandang asal, respeto, at pakikipagtulungan sa iba.",
                            "Patuloy na umuunlad sa akademiko at personal na pag-uugali.",
                            "Nakakamit ang karamihan sa mga inaasahang kasanayan sa baitang.",
                            "Maayos na natatapos ang mga gawain sa itinakdang oras.",
                            "May positibong pananaw at interes sa pag-aaral.",
                            "Mahusay niyang naipapakita ang kanyang kakayahan sa iba’t ibang gawain.",
                            "Patuloy na nagsisikap upang mapabuti pa ang kanyang pagganap.",
                            "Nagpapakita ng pagiging masipag at responsable bilang mag-aaral.",
                            "May kakayahang gamitin ang natutunan sa mga praktikal na sitwasyon.",
                            "Patuloy na nagpapakita ng pag-unlad at kahusayan sa klase.",
                            "Ipagpatuloy ang magandang gawain upang makamit ang higit pang tagumpay."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga aralin at pangunahing kasanayan.",
                            "Patuloy na nagsisikap upang mapabuti ang kanyang pagganap sa klase.",
                            "Nakikilahok sa mga gawain at sumusunod sa mga panuto ng guro.",
                            "May kakayahang matuto at mapaunlad pa ang kanyang mga kasanayan.",
                            "Ginagawa ang mga gawain ngunit nangangailangan pa ng kaunting gabay.",
                            "Patuloy na nagpapakita ng pag-unlad sa iba’t ibang asignatura.",
                            "Kailangan lamang ng dagdag na pagsasanay upang maging mas mahusay.",
                            "Nagpapakita ng magandang pakikitungo at pakikipagtulungan sa klase.",
                            "May sapat na kaalaman sa mga paksang pinag-aaralan.",
                            "Nakikinig sa talakayan at nagsisikap na maunawaan ang mga aralin.",
                            "Patuloy na pinauunlad ang kanyang kakayahan sa pamamagitan ng pagsasanay.",
                            "Nagpapakita ng interes sa pag-aaral at pagtanggap ng mga hamon.",
                            "Nakakamit ang karamihan sa mga layunin sa pagkatuto.",
                            "Kailangan lamang ng patuloy na paggabay upang higit pang umunlad.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang mas mataas na antas ng pagkatuto."
                        },
                        new[]
                        {
                            "Nangangailangan pa ng dagdag na pagsasanay upang mapaunlad ang kanyang mga kasanayan.",
                            "Kailangan ng higit na paggabay upang lubos na maunawaan ang mga aralin.",
                            "Hikayatin siyang maging mas aktibo sa mga gawain at talakayan.",
                            "Nangangailangan ng karagdagang pagsisikap upang mapabuti ang kanyang pagganap.",
                            "Kailangan pang pagbutihin ang pagsunod sa mga panuto at paggawa ng gawain.",
                            "May kakayahang umunlad kung patuloy na magsisikap sa pag-aaral.",
                            "Nangangailangan ng suporta upang mapaunlad ang kanyang tiwala sa sarili.",
                            "Kailangan ng mas regular na pagsasanay upang makamit ang inaasahang kasanayan.",
                            "Dapat pagtuunan ng pansin ang pagkumpleto ng mga gawain sa oras.",
                            "Patuloy na gabayan upang maging mas responsable sa kanyang pag-aaral.",
                            "Nangangailangan ng dagdag na oras sa pag-aaral at pagsasanay.",
                            "May mga kasanayang natutuhan ngunit kailangan pa ng pagpapaunlad.",
                            "Hikayatin siyang maging mas determinado at masipag sa pag-aaral.",
                            "Kailangan ng patuloy na tulong upang mapabuti ang akademikong pagganap.",
                            "Sa patuloy na paggabay at pagsisikap, makakamit niya ang inaasahang pag-unlad."
                        },
                        new[]
                        {
                            "Nangangailangan ng mas maraming gabay upang mapaunlad ang mga pangunahing kasanayan.",
                            "Kailangan ng karagdagang pagsasanay upang mas maunawaan ang mga aralin.",
                            "Hikayatin ang regular na pag-aaral at pagsasanay sa loob at labas ng klase.",
                            "Nangangailangan ng suporta upang mapabuti ang kanyang pagganap sa mga gawain.",
                            "Kailangan pang paunlarin ang pagiging responsable at masipag sa pag-aaral.",
                            "Dapat pagtuunan ng pansin ang mga kasanayang hindi pa lubos na natutuhan.",
                            "Nangangailangan ng patuloy na paggabay mula sa guro at magulang.",
                            "Kailangan ng higit na pakikilahok sa mga aktibidad sa klase.",
                            "Hikayatin na magkaroon ng positibong saloobin at interes sa pag-aaral.",
                            "Nangangailangan ng dagdag na pagsasanay upang mapaunlad ang kanyang kakayahan.",
                            "Kailangan pang pagbutihin ang konsentrasyon at pagtutok sa mga gawain.",
                            "Patuloy na tutulungan upang mapaunlad ang kanyang kaalaman at kasanayan.",
                            "Dapat magsikap nang higit upang makamit ang inaasahang pamantayan.",
                            "Kailangan ng regular na suporta at paggabay upang makamit ang pag-unlad.",
                            "Sa pamamagitan ng pagsisikap, pagsasanay, at suporta, makakamit niya ang mas mahusay na pagganap."
                        }
                    ),
                    [5] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging kahusayan sa akademiko at patuloy na nakakamit ang mataas na antas ng pagkatuto.",
                            "Napakahusay niyang naipapakita ang kanyang kaalaman, kasanayan, at magandang pag-uugali sa klase.",
                            "Palaging aktibo, masipag, at responsable sa lahat ng gawaing pang-akademiko.",
                            "Nakakamit ang mga layunin sa pagkatuto nang higit pa sa inaasahang pamantayan.",
                            "Nagpapakita ng mahusay na pag-iisip, pagkamalikhain, at kakayahang lumutas ng mga suliranin.",
                            "Palaging handa at nagbibigay ng pinakamahusay na pagsisikap sa bawat gawain.",
                            "Ipinapakita ang pagiging huwarang mag-aaral sa pamamagitan ng magandang asal at disiplina.",
                            "Mahusay makipagtulungan at nagpapakita ng positibong impluwensya sa kanyang mga kaklase.",
                            "Nagpapakita ng mataas na antas ng kumpiyansa at pagiging responsable sa pag-aaral.",
                            "Mahusay na naiaangkop ang mga natutunan sa iba’t ibang sitwasyon.",
                            "Patuloy na nagpapakita ng kahusayan sa mga pagsusulit, proyekto, at iba pang gawain.",
                            "May malaking potensyal at patuloy na pinauunlad ang sariling kakayahan.",
                            "Ipinagmamalaki ang kanyang dedikasyon at positibong pananaw sa pag-aaral.",
                            "Isang huwarang mag-aaral na nagpapakita ng sipag, tiyaga, at determinasyon.",
                            "Ipagpatuloy ang kahanga-hangang pagganap at pagiging inspirasyon sa iba."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagsisikap upang mapabuti pa ang sarili.",
                            "Naipapakita ang magandang pag-unawa sa mga aralin at kasanayang tinatalakay.",
                            "Responsable sa paggawa ng mga gawain at maayos na sumusunod sa mga panuto.",
                            "Aktibong nakikilahok sa mga talakayan at pangkatang gawain.",
                            "Nagpapakita ng magandang asal at paggalang sa guro at kapwa mag-aaral.",
                            "Patuloy na umuunlad sa iba’t ibang larangan ng pag-aaral.",
                            "Nakakamit ang karamihan sa mga inaasahang kasanayan sa baitang.",
                            "Maayos na natatapos ang mga gawain sa itinakdang oras.",
                            "Nagpapakita ng interes at positibong saloobin sa pagkatuto.",
                            "May kakayahang gamitin ang natutunan sa praktikal na paraan.",
                            "Patuloy na nagsisikap upang makamit ang mas mataas na antas ng tagumpay.",
                            "Mahusay makipagtulungan sa mga kaklase at guro.",
                            "Nagpapakita ng pagiging masipag at maaasahan sa mga gawain.",
                            "May magandang pag-unlad sa akademiko at pag-uugali.",
                            "Ipagpatuloy ang magandang pagsisikap at dedikasyon sa pag-aaral."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga aralin at pangunahing kasanayan.",
                            "Patuloy na nagsisikap upang mapaunlad ang kanyang kakayahan sa pag-aaral.",
                            "Nakikilahok sa mga gawain at sumusunod sa mga panuto ng guro.",
                            "May kakayahang matuto at umunlad sa pamamagitan ng patuloy na pagsasanay.",
                            "Ginagawa ang mga gawain ngunit nangangailangan pa ng kaunting gabay.",
                            "Patuloy na nagpapakita ng pag-unlad sa iba’t ibang asignatura.",
                            "Kailangan lamang ng dagdag na pagsasanay upang maging mas mahusay.",
                            "Nagpapakita ng magandang pakikitungo at pakikipagtulungan sa klase.",
                            "May sapat na kaalaman sa mga paksang tinatalakay.",
                            "Nakikinig sa talakayan at nagsisikap na maunawaan ang mga aralin.",
                            "Patuloy na pinauunlad ang kanyang kasanayan sa pamamagitan ng pagsasanay.",
                            "May positibong pananaw sa pag-aaral at pagtanggap ng mga hamon.",
                            "Nakakamit ang mga pangunahing layunin sa pagkatuto.",
                            "Nangangailangan lamang ng patuloy na paggabay upang higit pang umunlad.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang mas mataas na tagumpay."
                        },
                        new[]
                        {
                            "Nangangailangan pa ng dagdag na pagsasanay upang mapaunlad ang kanyang mga kasanayan.",
                            "Kailangan ng higit na paggabay upang lubos na maunawaan ang mga aralin.",
                            "Hikayatin siyang maging mas aktibo sa mga gawain at talakayan.",
                            "Nangangailangan ng karagdagang pagsisikap upang mapabuti ang kanyang pagganap.",
                            "Kailangan pang pagbutihin ang paggawa at pagsusumite ng mga gawain.",
                            "May kakayahang umunlad kung patuloy na magsisikap at magsasanay.",
                            "Nangangailangan ng suporta upang magkaroon ng higit na tiwala sa sarili.",
                            "Kailangan ng mas regular na pag-aaral upang mapaunlad ang kaalaman.",
                            "Dapat pagtuunan ng pansin ang mga kasanayang hindi pa lubos na natutuhan.",
                            "Patuloy na gabayan upang maging mas responsable sa mga gawain.",
                            "Nangangailangan ng dagdag na oras at pagsisikap sa pag-aaral.",
                            "May mga natutunang kasanayan ngunit kailangan pa ng pagpapaunlad.",
                            "Hikayatin na maging mas determinado sa pagkamit ng mga layunin.",
                            "Kailangan ng patuloy na suporta mula sa guro at magulang.",
                            "Sa patuloy na paggabay at pagsisikap, makakamit niya ang inaasahang pag-unlad."
                        },
                        new[]
                        {
                            "Nangangailangan ng masusing paggabay upang mapaunlad ang mga pangunahing kasanayan.",
                            "Kailangan ng karagdagang pagsasanay upang mas maunawaan ang mga aralin.",
                            "Hikayatin ang regular na pag-aaral at pagsasanay upang mapabuti ang pagganap.",
                            "Nangangailangan ng tulong upang makumpleto nang maayos ang mga gawain.",
                            "Kailangan pang paunlarin ang pagiging responsable at masipag sa pag-aaral.",
                            "Dapat bigyang-pansin ang mga kasanayang nangangailangan ng pagpapabuti.",
                            "Nangangailangan ng patuloy na suporta mula sa guro at magulang.",
                            "Kailangan ng mas aktibong pakikilahok sa mga gawain sa klase.",
                            "Hikayatin na magkaroon ng positibong saloobin at interes sa pagkatuto.",
                            "Nangangailangan ng dagdag na pagsasanay upang mapaunlad ang kaalaman at kakayahan.",
                            "Kailangan pang pagbutihin ang konsentrasyon at pagsunod sa mga panuto.",
                            "Patuloy na tutulungan upang mapaunlad ang kanyang akademiko at personal na kakayahan.",
                            "Dapat magsikap nang higit upang makamit ang inaasahang pamantayan.",
                            "Kailangan ng regular na paggabay at pagsasanay upang magkaroon ng pag-unlad.",
                            "Sa tulong ng pagsisikap, suporta, at determinasyon, makakamit niya ang mas mahusay na pagganap."
                        }
                    ),
                    [6] = CreateGradeRemarks(
                        new[]
                        {
                            "Ipinapakita ng mag-aaral ang natatanging kahusayan sa akademiko at patuloy na nakakamit ang pinakamataas na antas ng pagkatuto.",
                            "Napakahusay niyang naipapakita ang kaalaman, kasanayan, at magandang asal sa lahat ng gawain.",
                            "Palaging masipag, responsable, at determinado sa pagkamit ng mga layunin sa pag-aaral.",
                            "Nakakamit ang mga pamantayan sa pagkatuto nang higit pa sa inaasahan.",
                            "Nagpapakita ng mahusay na kritikal na pag-iisip at kakayahang lutasin ang iba’t ibang suliranin.",
                            "Aktibo sa klase at nagbibigay ng makabuluhang kontribusyon sa mga talakayan at gawain.",
                            "Ipinapakita ang pagiging huwarang mag-aaral sa pamamagitan ng disiplina, respeto, at pananagutan.",
                            "Mahusay niyang naiaangkop ang mga natutunan sa pang-araw-araw na sitwasyon.",
                            "Patuloy na nagpapakita ng mataas na kalidad ng paggawa sa mga proyekto at takdang-aralin.",
                            "Nagpapakita ng pagiging malikhain, mapanuri, at may tiwala sa sariling kakayahan.",
                            "Mahusay makipagtulungan at nagsisilbing inspirasyon sa mga kaklase.",
                            "Ipinapakita ang dedikasyon at positibong pananaw sa pag-aaral.",
                            "Patuloy na nagpapaunlad ng talento at kakayahan sa iba’t ibang larangan.",
                            "Isang huwarang mag-aaral na nagpapakita ng sipag, tiyaga, at determinasyon.",
                            "Ipagpatuloy ang kahanga-hangang pagganap at pagiging modelo sa klase."
                        },
                        new[]
                        {
                            "Mahusay ang kanyang pagganap at patuloy na nagsisikap upang higit pang umunlad.",
                            "Naipapakita ang mahusay na pag-unawa sa mga aralin at kasanayang tinatalakay.",
                            "Responsable sa paggawa ng mga gawain at maayos na sumusunod sa mga panuto.",
                            "Aktibong nakikilahok sa mga talakayan at pangkatang gawain.",
                            "Nagpapakita ng mabuting asal, respeto, at pakikipagtulungan sa iba.",
                            "Patuloy na nagpapakita ng magandang pag-unlad sa akademiko at pag-uugali.",
                            "Nakakamit ang karamihan sa mga inaasahang kasanayan sa baitang.",
                            "Maayos na natatapos ang mga gawain sa itinakdang oras.",
                            "May positibong saloobin at interes sa pagkatuto.",
                            "Nagagamit nang maayos ang mga natutunang kaalaman at kasanayan.",
                            "Patuloy na nagpapakita ng kasipagan at pagiging responsable.",
                            "Mahusay makibahagi sa mga gawain at proyekto ng klase.",
                            "May kakayahang harapin ang mga hamon sa pag-aaral.",
                            "Patuloy na pinauunlad ang sariling kakayahan at talento.",
                            "Ipagpatuloy ang magandang pagganap at pagsisikap sa pag-aaral."
                        },
                        new[]
                        {
                            "Naipapakita ang sapat na pag-unawa sa mga aralin at pangunahing kasanayan.",
                            "Patuloy na nagsisikap upang mapabuti ang kanyang pagganap sa klase.",
                            "Nakikilahok sa mga gawain at sumusunod sa mga panuto ng guro.",
                            "May kakayahang matuto at umunlad sa pamamagitan ng patuloy na pagsasanay.",
                            "Ginagawa ang mga gawain ngunit nangangailangan pa ng kaunting paggabay.",
                            "Patuloy na nagpapakita ng pag-unlad sa iba’t ibang asignatura.",
                            "Kailangan lamang ng dagdag na pagsasanay upang maging mas mahusay.",
                            "Nagpapakita ng magandang pakikitungo sa guro at mga kaklase.",
                            "May sapat na kaalaman sa mga paksang tinatalakay.",
                            "Nakikinig sa talakayan at nagsisikap na maunawaan ang mga aralin.",
                            "Patuloy na pinauunlad ang kanyang mga kasanayan sa pamamagitan ng pagsasanay.",
                            "May positibong pananaw sa pag-aaral at pagtanggap sa mga hamon.",
                            "Nakakamit ang karamihan sa mga pangunahing layunin sa pagkatuto.",
                            "Nangangailangan lamang ng patuloy na gabay upang higit pang umunlad.",
                            "Ipagpatuloy ang pagsisikap upang makamit ang mas mataas na antas ng pagkatuto."
                        },
                        new[]
                        {
                            "Nangangailangan pa ng karagdagang pagsasanay upang mapaunlad ang kanyang mga kasanayan.",
                            "Kailangan ng higit na paggabay upang lubos na maunawaan ang mga aralin.",
                            "Hikayatin siyang maging mas aktibo sa mga gawain at talakayan.",
                            "Nangangailangan ng dagdag na pagsisikap upang mapabuti ang kanyang pagganap.",
                            "Kailangan pang pagbutihin ang paggawa at pagsusumite ng mga gawain.",
                            "May kakayahang umunlad kung patuloy na magsisikap at magsasanay.",
                            "Nangangailangan ng suporta upang magkaroon ng higit na tiwala sa sarili.",
                            "Kailangan ng mas regular na pag-aaral upang mapalawak ang kaalaman.",
                            "Dapat pagtuunan ng pansin ang mga kasanayang hindi pa lubos na natutuhan.",
                            "Patuloy na gabayan upang maging mas responsable sa kanyang pag-aaral.",
                            "Nangangailangan ng dagdag na oras at pagsisikap upang makamit ang inaasahang antas.",
                            "May mga natutunang kasanayan ngunit kailangan pa ng pagpapaunlad.",
                            "Hikayatin siyang maging mas determinado sa pagkamit ng kanyang mga layunin.",
                            "Kailangan ng patuloy na suporta mula sa guro at magulang.",
                            "Sa pamamagitan ng patuloy na pagsisikap, makakamit niya ang higit na pag-unlad."
                        },
                        new[]
                        {
                            "Nangangailangan ng masusing paggabay upang mapaunlad ang mga pangunahing kasanayan.",
                            "Kailangan ng karagdagang pagsasanay upang mas maunawaan ang mahahalagang konsepto.",
                            "Hikayatin ang regular na pag-aaral at pagsasanay upang mapabuti ang kanyang pagganap.",
                            "Nangangailangan ng tulong upang makumpleto nang maayos ang mga gawain.",
                            "Kailangan pang paunlarin ang pagiging responsable at masipag sa pag-aaral.",
                            "Dapat bigyang-pansin ang mga kasanayang nangangailangan ng pagpapabuti.",
                            "Nangangailangan ng patuloy na suporta at gabay mula sa guro at magulang.",
                            "Kailangan ng mas aktibong pakikilahok sa mga gawain sa klase.",
                            "Hikayatin na magkaroon ng positibong pananaw at interes sa pag-aaral.",
                            "Nangangailangan ng dagdag na pagsasanay upang mapaunlad ang kaalaman at kakayahan.",
                            "Kailangan pang pagbutihin ang konsentrasyon, disiplina, at pagsunod sa mga panuto.",
                            "Patuloy na tutulungan upang mapaunlad ang akademiko at personal na kakayahan.",
                            "Dapat magsikap nang higit upang makamit ang mga inaasahang pamantayan.",
                            "Kailangan ng regular na paggabay at pagsasanay upang magkaroon ng makabuluhang pag-unlad.",
                            "Sa tulong ng pagsisikap, suporta, at determinasyon, makakamit niya ang mas mahusay na pagganap."
                        }
                    )
                };

        public static IReadOnlyList<string> GetRemarks(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel)
        {
            if (!RemarksByGrade.TryGetValue(
                    gradeLevel,
                    out IReadOnlyDictionary<AutomatedRemarkLevel, string[]>?
                        gradeRemarks) ||
                !gradeRemarks.TryGetValue(
                    performanceLevel,
                    out string[]? remarks))
            {
                return Array.Empty<string>();
            }

            return remarks;
        }

        private static IReadOnlyDictionary<
            AutomatedRemarkLevel,
            string[]> CreateGradeRemarks(
                string[] advancing,
                string[] benchmarking,
                string[] connecting,
                string[] developing,
                string[] emerging)
        {
            return new Dictionary<AutomatedRemarkLevel, string[]>
            {
                [AutomatedRemarkLevel.Advancing] = advancing,
                [AutomatedRemarkLevel.Benchmarking] = benchmarking,
                [AutomatedRemarkLevel.Connecting] = connecting,
                [AutomatedRemarkLevel.Developing] = developing,
                [AutomatedRemarkLevel.Emerging] = emerging
            };
        }
    }
}

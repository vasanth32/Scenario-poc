û,
ƒD:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\PaymentService\Controllers\PaymentsController.cs
	namespace 	
PaymentService
 
. 
Controllers $
;$ %
[ 
ApiController 
] 
[		 
Route		 
(		 
$str		 
)		 
]		 
public

 
class

 
PaymentsController

 
:

  !
ControllerBase

" 0
{ 
private 
readonly 
PaymentDbContext %
_context& .
;. /
private 
readonly 
ILogger 
< 
PaymentsController /
>/ 0
_logger1 8
;8 9
public 

PaymentsController 
( 
PaymentDbContext .
context/ 6
,6 7
ILogger8 ?
<? @
PaymentsController@ R
>R S
loggerT Z
)Z [
{ 
_context 
= 
context 
; 
_logger 
= 
logger 
; 
} 
[ 
HttpPost 
( 
$str 
) 
] 
public 

async 
Task 
< 
ActionResult "
<" #
Payment# *
>* +
>+ ,
ProcessPayment- ;
(; <
[< =
FromBody= E
]E F!
ProcessPaymentRequestG \
request] d
)d e
{ 
_logger 
. 
LogInformation 
( 
$str ]
,] ^
request 
. 
OrderId 
, 
request $
.$ %
Amount% +
)+ ,
;, -
var 
payment 
= 
new 
Payment !
{   	
OrderId!! 
=!! 
request!! 
.!! 
OrderId!! %
,!!% &
Amount"" 
="" 
request"" 
."" 
Amount"" #
,""# $
PaymentMethod## 
=## 
request## #
.### $
PaymentMethod##$ 1
,##1 2
Status$$ 
=$$ 
$str$$ !
,$$! "
TransactionId%% 
=%% 
Guid%%  
.%%  !
NewGuid%%! (
(%%( )
)%%) *
.%%* +
ToString%%+ 3
(%%3 4
)%%4 5
}&& 	
;&&	 

_context(( 
.(( 
Payments(( 
.(( 
Add(( 
((( 
payment(( %
)((% &
;((& '
await)) 
_context)) 
.)) 
SaveChangesAsync)) '
())' (
)))( )
;))) *
await,, 
Task,, 
.,, 
Delay,, 
(,, 
$num,, 
),, 
;,, 
payment// 
.// 
Status// 
=// 
$str// $
;//$ %
payment00 
.00 
ProcessedAt00 
=00 
DateTime00 &
.00& '
UtcNow00' -
;00- .
await11 
_context11 
.11 
SaveChangesAsync11 '
(11' (
)11( )
;11) *
_logger33 
.33 
LogInformation33 
(33 
$str33 y
,33y z
payment44 
.44 
Id44 
,44 
payment44 
.44  
TransactionId44  -
)44- .
;44. /
return66 
Ok66 
(66 
payment66 
)66 
;66 
}77 
[== 
HttpGet== 
(== 
$str== 
)== 
]== 
public>> 

async>> 
Task>> 
<>> 
ActionResult>> "
<>>" #
Payment>># *
>>>* +
>>>+ ,

GetPayment>>- 7
(>>7 8
int>>8 ;
id>>< >
)>>> ?
{?? 
_logger@@ 
.@@ 
LogInformation@@ 
(@@ 
$str@@ E
,@@E F
id@@G I
)@@I J
;@@J K
varBB 
paymentBB 
=BB 
awaitBB 
_contextBB $
.BB$ %
PaymentsBB% -
.BB- .
	FindAsyncBB. 7
(BB7 8
idBB8 :
)BB: ;
;BB; <
ifDD 

(DD 
paymentDD 
==DD 
nullDD 
)DD 
{EE 	
_loggerFF 
.FF 

LogWarningFF 
(FF 
$strFF F
,FFF G
idFFH J
)FFJ K
;FFK L
returnGG 
NotFoundGG 
(GG 
)GG 
;GG 
}HH 	
returnJJ 
OkJJ 
(JJ 
paymentJJ 
)JJ 
;JJ 
}KK 
}LL 
publicNN 
classNN !
ProcessPaymentRequestNN "
{OO 
publicPP 

intPP 
OrderIdPP 
{PP 
getPP 
;PP 
setPP !
;PP! "
}PP# $
publicQQ 

decimalQQ 
AmountQQ 
{QQ 
getQQ 
;QQ  
setQQ! $
;QQ$ %
}QQ& '
publicRR 

stringRR 
PaymentMethodRR 
{RR  !
getRR" %
;RR% &
setRR' *
;RR* +
}RR, -
=RR. /
stringRR0 6
.RR6 7
EmptyRR7 <
;RR< =
}SS «
zD:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\PaymentService\Data\PaymentDbContext.cs
	namespace 	
PaymentService
 
. 
Data 
; 
public 
class 
PaymentDbContext 
: 
	DbContext  )
{ 
public 

PaymentDbContext 
( 
DbContextOptions ,
<, -
PaymentDbContext- =
>= >
options? F
)F G
:H I
baseJ N
(N O
optionsO V
)V W
{		 
}

 
public 

DbSet 
< 
Payment 
> 
Payments "
{# $
get% (
;( )
set* -
;- .
}/ 0
	protected 
override 
void 
OnModelCreating +
(+ ,
ModelBuilder, 8
modelBuilder9 E
)E F
{ 
base 
. 
OnModelCreating 
( 
modelBuilder )
)) *
;* +
modelBuilder 
. 
Entity 
< 
Payment #
># $
($ %
entity% +
=>, .
{ 	
entity 
. 
HasKey 
( 
e 
=> 
e  
.  !
Id! #
)# $
;$ %
entity 
. 
Property 
( 
e 
=>  
e! "
." #
PaymentMethod# 0
)0 1
.1 2

IsRequired2 <
(< =
)= >
.> ?
HasMaxLength? K
(K L
$numL N
)N O
;O P
entity 
. 
Property 
( 
e 
=>  
e! "
." #
Status# )
)) *
.* +

IsRequired+ 5
(5 6
)6 7
.7 8
HasMaxLength8 D
(D E
$numE G
)G H
;H I
entity 
. 
Property 
( 
e 
=>  
e! "
." #
TransactionId# 0
)0 1
.1 2
HasMaxLength2 >
(> ?
$num? B
)B C
;C D
entity 
. 
Property 
( 
e 
=>  
e! "
." #
Amount# )
)) *
.* +

IsRequired+ 5
(5 6
)6 7
.7 8
HasPrecision8 D
(D E
$numE G
,G H
$numI J
)J K
;K L
entity 
. 
HasIndex 
( 
e 
=>  
e! "
." #
OrderId# *
)* +
;+ ,
entity 
. 
HasIndex 
( 
e 
=>  
e! "
." #
TransactionId# 0
)0 1
;1 2
entity 
. 
HasIndex 
( 
e 
=>  
e! "
." #
Status# )
)) *
;* +
} 	
)	 

;
 
} 
} ‹
†D:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\PaymentService\Middleware\ResponseTimeMiddleware.cs
	namespace 	
PaymentService
 
. 

Middleware #
;# $
public 
class "
ResponseTimeMiddleware #
{		 
private

 
readonly

 
RequestDelegate

 $
_next

% *
;

* +
private 
readonly 
ILogger 
< "
ResponseTimeMiddleware 3
>3 4
_logger5 <
;< =
private 
readonly 
TelemetryClient $
?$ %
_telemetryClient& 6
;6 7
private 
const 
long "
SlowRequestThresholdMs -
=. /
$num0 3
;3 4
public 
"
ResponseTimeMiddleware !
(! "
RequestDelegate 
next 
, 
ILogger 
< "
ResponseTimeMiddleware &
>& '
logger( .
,. /
TelemetryClient 
? 
telemetryClient (
=) *
null+ /
)/ 0
{ 
_next 
= 
next 
; 
_logger 
= 
logger 
; 
_telemetryClient 
= 
telemetryClient *
;* +
} 
public 

async 
Task 
InvokeAsync !
(! "
HttpContext" -
context. 5
)5 6
{ 
var 
	stopwatch 
= 
System 
. 
Diagnostics *
.* +
	Stopwatch+ 4
.4 5
StartNew5 =
(= >
)> ?
;? @
await 
_next 
( 
context 
) 
; 
	stopwatch 
. 
Stop 
( 
) 
; 
var 
elapsedMilliseconds 
=  !
	stopwatch" +
.+ ,
ElapsedMilliseconds, ?
;? @
_telemetryClient"" 
?"" 
."" 
TrackMetric"" %
(""% &
$str""& 9
,""9 :
elapsedMilliseconds""; N
)""N O
;""O P
if$$ 

($$ 
elapsedMilliseconds$$ 
>$$  !"
SlowRequestThresholdMs$$" 8
)$$8 9
{%% 	
_logger&& 
.&& 

LogWarning&& 
(&& 
$str'' z
,''z {
context(( 
.(( 
Request(( 
.((  
Method((  &
,((& '
context)) 
.)) 
Request)) 
.))  
Path))  $
,))$ %
context** 
.** 
Response**  
.**  !

StatusCode**! +
,**+ ,
elapsedMilliseconds++ #
)++# $
;++$ %
},, 	
context.. 
... 
Response.. 
... 
Headers..  
...  !
Append..! '
(..' (
$str..( 9
,..9 :
$"..; =
{..= >
elapsedMilliseconds..> Q
}..Q R
$str..R T
"..T U
)..U V
;..V W
}// 
}00 Ì
sD:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\PaymentService\Models\Payment.cs
	namespace 	
PaymentService
 
. 
Models 
;  
public 
class 
Payment 
{ 
public 

int 
Id 
{ 
get 
; 
set 
; 
} 
public 

int 
OrderId 
{ 
get 
; 
set !
;! "
}# $
public 

decimal 
Amount 
{ 
get 
;  
set! $
;$ %
}& '
public 

string 
PaymentMethod 
{  !
get" %
;% &
set' *
;* +
}, -
=. /
string0 6
.6 7
Empty7 <
;< =
public		 

string		 
Status		 
{		 
get		 
;		 
set		  #
;		# $
}		% &
=		' (
$str		) 2
;		2 3
public

 

string

 
TransactionId

 
{

  !
get

" %
;

% &
set

' *
;

* +
}

, -
=

. /
string

0 6
.

6 7
Empty

7 <
;

< =
public 

DateTime 
	CreatedAt 
{ 
get  #
;# $
set% (
;( )
}* +
=, -
DateTime. 6
.6 7
UtcNow7 =
;= >
public 

DateTime 
? 
ProcessedAt  
{! "
get# &
;& '
set( +
;+ ,
}- .
} „E
lD:\PracticeProjects\Scenario-poc\Improving Microservice Latency Under High Traffic\PaymentService\Program.cs
var 
builder 
= 
WebApplication 
. 
CreateBuilder *
(* +
args+ /
)/ 0
;0 1
Log 
. 
Logger 

= 
new 
LoggerConfiguration $
($ %
)% &
.		 
ReadFrom		 
.		 
Configuration		 
(		 
builder		 #
.		# $
Configuration		$ 1
)		1 2
.

 
Enrich

 
.

 
FromLogContext

 
(

 
)

 
. 
Enrich 
. 
WithProperty 
( 
$str "
," #
$str$ 4
)4 5
. 
WriteTo 
. 
Console 
( 
) 
. 
WriteTo 
. 
File 
( 
$str ,
,, -
rollingInterval. =
:= >
RollingInterval? N
.N O
DayO R
)R S
. 
CreateLogger 
( 
) 
; 
builder 
. 
Host 
. 

UseSerilog 
( 
) 
; 
builder 
. 
Services 
. 
AddControllers 
(  
)  !
;! "
builder 
. 
Services 
. #
AddEndpointsApiExplorer (
(( )
)) *
;* +
builder 
. 
Services 
. 
AddSwaggerGen 
( 
)  
;  !
builder 
. 
Services 
. +
AddApplicationInsightsTelemetry 0
(0 1
)1 2
;2 3
builder$$ 
.$$ 
Services$$ 
.$$ 
AddDbContext$$ 
<$$ 
PaymentDbContext$$ .
>$$. /
($$/ 0
options$$0 7
=>$$8 :
{%% 
var&& 
connectionString&& 
=&& 
builder&& "
.&&" #
Configuration&&# 0
.&&0 1
GetConnectionString&&1 D
(&&D E
$str&&E X
)&&X Y
??'' 

$str'' $
;''$ %
options)) 
.)) 
	UseSqlite)) 
()) 
connectionString)) &
,))& '
sqliteOptions))( 5
=>))6 8
{** 
sqliteOptions,, 
.,, 
CommandTimeout,, $
(,,$ %
$num,,% '
),,' (
;,,( )
}-- 
)-- 
;-- 
if00 
(00 
builder00 
.00 
Environment00 
.00 
IsDevelopment00 )
(00) *
)00* +
)00+ ,
{11 
options22 
.22 
LogTo22 
(22 
Console22 
.22 
	WriteLine22 '
,22' (
LogLevel22) 1
.221 2
Information222 =
)22= >
.33 &
EnableSensitiveDataLogging33 '
(33' (
false33( -
)33- .
.44  
EnableDetailedErrors44 !
(44! "
)44" #
;44# $
}55 
}66 
)66 
;66 
builder99 
.99 
Services99 
.99 
AddHealthChecks99  
(99  !
)99! "
.:: 
AddDbContextCheck:: 
<:: 
PaymentDbContext:: '
>::' (
(::( )
)::) *
;::* +
builder@@ 
.@@ 
Services@@ 
.@@ "
AddResponseCompression@@ '
(@@' (
options@@( /
=>@@0 2
{AA 
optionsBB 
.BB 
EnableForHttpsBB 
=BB 
trueBB !
;BB! "
optionsCC 
.CC 
	ProvidersCC 
.CC 
AddCC 
<CC 
	MicrosoftCC #
.CC# $

AspNetCoreCC$ .
.CC. /
ResponseCompressionCC/ B
.CCB C%
BrotliCompressionProviderCCC \
>CC\ ]
(CC] ^
)CC^ _
;CC_ `
optionsDD 
.DD 
	ProvidersDD 
.DD 
AddDD 
<DD 
	MicrosoftDD #
.DD# $

AspNetCoreDD$ .
.DD. /
ResponseCompressionDD/ B
.DDB C#
GzipCompressionProviderDDC Z
>DDZ [
(DD[ \
)DD\ ]
;DD] ^
optionsEE 
.EE 
	MimeTypesEE 
=EE 
	MicrosoftEE !
.EE! "

AspNetCoreEE" ,
.EE, -
ResponseCompressionEE- @
.EE@ A'
ResponseCompressionDefaultsEEA \
.EE\ ]
	MimeTypesEE] f
.EEf g
ConcatEEg m
(EEm n
newFF 
[FF 
]FF 
{FF 
$strFF "
,FF" #
$strFF$ 5
}FF6 7
)FF7 8
;FF8 9
}GG 
)GG 
;GG 
builderII 
.II 
ServicesII 
.II 
	ConfigureII 
<II 
	MicrosoftII $
.II$ %

AspNetCoreII% /
.II/ 0
ResponseCompressionII0 C
.IIC D,
 BrotliCompressionProviderOptionsIID d
>IId e
(IIe f
optionsIIf m
=>IIn p
{JJ 
optionsKK 
.KK 
LevelKK 
=KK 
SystemKK 
.KK 
IOKK 
.KK 
CompressionKK )
.KK) *
CompressionLevelKK* :
.KK: ;
OptimalKK; B
;KKB C
}LL 
)LL 
;LL 
builderNN 
.NN 
ServicesNN 
.NN 
	ConfigureNN 
<NN 
	MicrosoftNN $
.NN$ %

AspNetCoreNN% /
.NN/ 0
ResponseCompressionNN0 C
.NNC D*
GzipCompressionProviderOptionsNND b
>NNb c
(NNc d
optionsNNd k
=>NNl n
{OO 
optionsPP 
.PP 
LevelPP 
=PP 
SystemPP 
.PP 
IOPP 
.PP 
CompressionPP )
.PP) *
CompressionLevelPP* :
.PP: ;
OptimalPP; B
;PPB C
}QQ 
)QQ 
;QQ 
varSS 
appSS 
=SS 	
builderSS
 
.SS 
BuildSS 
(SS 
)SS 
;SS 
ifVV 
(VV 
appVV 
.VV 
EnvironmentVV 
.VV 
IsDevelopmentVV !
(VV! "
)VV" #
)VV# $
{WW 
appXX 
.XX 

UseSwaggerXX 
(XX 
)XX 
;XX 
appYY 
.YY 
UseSwaggerUIYY 
(YY 
)YY 
;YY 
}ZZ 
app\\ 
.\\ 
UseHttpsRedirection\\ 
(\\ 
)\\ 
;\\ 
app__ 
.__ 
UseMiddleware__ 
<__ 
PaymentService__  
.__  !

Middleware__! +
.__+ ,"
ResponseTimeMiddleware__, B
>__B C
(__C D
)__D E
;__E F
appbb 
.bb "
UseResponseCompressionbb 
(bb 
)bb 
;bb 
appdd 
.dd 
UseAuthorizationdd 
(dd 
)dd 
;dd 
appff 
.ff 
MapControllersff 
(ff 
)ff 
;ff 
appii 
.ii 
MapHealthChecksii 
(ii 
$strii 
)ii 
;ii 
usingll 
(ll 
varll 

scopell 
=ll 
appll 
.ll 
Servicesll 
.ll  
CreateScopell  +
(ll+ ,
)ll, -
)ll- .
{mm 
varnn 
dbnn 

=nn 
scopenn 
.nn 
ServiceProvidernn "
.nn" #
GetRequiredServicenn# 5
<nn5 6
PaymentDbContextnn6 F
>nnF G
(nnG H
)nnH I
;nnI J
dboo 
.oo 
Databaseoo 
.oo 
EnsureCreatedoo 
(oo 
)oo 
;oo  
}pp 
Logrr 
.rr 
Informationrr 
(rr 
$strrr ,
)rr, -
;rr- .
apptt 
.tt 
Runtt 
(tt 
)tt 	
;tt	 

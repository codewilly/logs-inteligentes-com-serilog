- TUTORIAL SEM IMAGENS TEMPORARIAMENTE

**Logs inteligentes com Serilog**

Uma das ferramentas mais populares para o registro de logs é o Serilog. Após configurado, um simples Log.Informationjá faz todo o trabalho sujo e persiste as informações em uma base de dados de nossa preferência. O problema é que não temos muito controle sobre a forma de salvar estes dados, o que pode resultar em uma grande massa de logs espalhados e/ou irrelevantes.

Ao final deste artigo, criaremos um simples projeto de API em .NET 6 com os seguintes recursos:

- Configuração e registro de logs com Serilog;
- Middleware para registro de logs de requisição e resposta;
- Agrupar e salvar logs quando conveniente;

**Obs.:** Iremos focar apenas no essencial. Boas práticas e padrões de projeto não serão levados em consideração, mas é recomendado seguir suas melhores práticas quando for implementar em seu projeto real.

**Criando o projeto**

Para este projeto, no Visual Studio (ou em sua IDE preferida) crie uma nova solution e adicione um projeto de API em NET 6 (você pode utilizar outras versões se desejar, mas o código pode não ser exatamente o mesmo). Em seguida, crie uma controller qualquer e adicione um endpoint GET.

![](RackMultipart20220819-1-h3za21_html_4b1fcd0651d61727.png)

**Configurando o Serilog**

No projeto da API, adicione os seguintes pacotes Nuget (instale os mais recentes):

- AspNetCore
- Sinks.Console

Se você já conhece o Serilog, sabe que existem vários sinks para salvar seus logs em algum lugar, como em arquivos de texto e em banco de dados (SQL Server, MongoDB, etc). Mas, **não queremos que o Serilog salve por conta própria,** por isso, vamos deixar apenas o console habilitado para que possamos ver a saída de log em algum lugar.

Em seguida, abra o Program.cs e vamos adicionar as configurações básicas para habilitar o uso do Serilog:

![](RackMultipart20220819-1-h3za21_html_12469bc3bea8972d.png)

Fique a vontade para adicionar outras configurações se julgar necessário. Aqui apenas suprimimos os logs da Microsoft e adicionamos o Sink do console.

Com isso já podemos começar a usar os logs. Para testarmos, no endpoint GET que criamos, adicione um Log.Information e execute o endpoint. Se tudo estiver correto, teremos o seguinte resultado:

![](RackMultipart20220819-1-h3za21_html_81e6103d595b4a20.png)

**Middleware de requisição e resposta**

Quando se trata de API, um dos logs mais importantes a serem salvos é a requisição junto da resposta da execução de um endpoint. É inviável adicionar esses logs manualmente em cada endpoint, pois além de poluirmos o projeto repetindo código, também corremos o risco de esquecer ou de salvar errado. Para contornar esse problema, criaremos um middleware!

Crie uma classe "LogMiddleware" e implemente a interface IMiddleware. A princípio, adicione o básico para o funcionamento do mesmo junto de um log para sabermos se está funcionando.

![](RackMultipart20220819-1-h3za21_html_4c26d3bb2ef06191.png)

Em seguida, precisamos configurar o uso do middleware. Em program.cs, adicione:

![](RackMultipart20220819-1-h3za21_html_6bbbf7d5700cd103.png)

Ao rodar o projeto e chamar nosso endpoint, teremos a seguinte saída no console:

![](RackMultipart20220819-1-h3za21_html_4ce8530505ec2fbf.png)
 Note que, toda vez que chamarmos nosso endpoint, a mensagem "Middleware Funcionando" será exibida antes mesmo de nosso HelloWorld.

Obs.: Futuramente adicionaremos filtros nas rotas para ignorar as chamadas do Swagger (por isto tem dois logs ao iniciar a aplicação).

Para mais informações sobre middlewares, consulte o artigo oficial da Microsoft: [https://docs.microsoft.com/pt-br/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0](https://docs.microsoft.com/pt-br/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0)

Com o esqueleto do middleware pronto, agora podemos finalmente começar a registrar os logs relevantes.

**HttpContextAcessor**

Utilizaremos a interface IHttpContextAccessor para termos acesso as informações da requisição e da resposta, mas primeiro, precisamos configurar seu uso na injeção de dependências. Novamente, em Program.cs, adicione: ![](RackMultipart20220819-1-h3za21_html_89761da2fba61037.png)

E no LogMiddleware, adicione um construtor junto da injeção:

![](RackMultipart20220819-1-h3za21_html_b56fe358972f1d74.png)

Conforme dito anteriormente, vamos adicionar um filtro para que as rotas do Swagger sejam ignoradas em nosso middleware, evitando um processamento desnecessário. Crie um método privado com o código abaixo: ![](RackMultipart20220819-1-h3za21_html_3e8fc6a5007ace87.png)
 Simplesmente criamos um array de rotas que queremos que seja ignorada e verificamos se o path recebido está presente na lista e deverá ser ignorado. Adicione aqui outras rotas se achar necessário.

Por fim, adicionamos a chamada do método no nosso InvokeAsync:

![](RackMultipart20220819-1-h3za21_html_20735760cbb50f79.png)

Aqui estamos pegando a rota chamada através do HttpContext obtido pelo HttpContextAcessor. Se for uma rota ignorada, simplesmente continuamos a execução do método pelo "await next" e retornamos.

Se você executar o projeto novamente, verá o console vazio até que você chame alguma rota.

![](RackMultipart20220819-1-h3za21_html_f6b0e35f78b238de.png)

**Logando a requisição e o retorno**

Finalmente, vamos registrar os logs da requisição e resposta de nosso endpoint. Iremos recuperar estas informações usando o HttpContext.

A princípio podemos recuperar o método (GET, POST, PUT, etc) junto da rota + query string e o status code (200, 400, 500, etc) da resposta:

![](RackMultipart20220819-1-h3za21_html_15ab4fd4c15e8a5d.png)

Neste exemplo, o endpoint não tem queryString, mas podemos adicionar um novo endpoint e testar:

![](RackMultipart20220819-1-h3za21_html_4391ce3bf71e5ef.png)

No caso do GET, estas informações já são suficientes para a maioria dos casos, mas, em um POST ou PUT, muitas vezes temos um payload atrelado, ou seja, mais dados sendo enviados. E claro, podemos registrar isto também.

Primeiro, crie um endpoint POST (ou PUT) e atribua um ViewModel a ele. Neste caso, a viewmodel tem apenas o nome e o documento:

![](RackMultipart20220819-1-h3za21_html_8c39215ac18b5508.png)

Se executar agora, não verá o payload informado pois ainda não configuramos o log disto. Para tal, voltamos ao nosso middleware e adicionamos o seguinte:

![](RackMultipart20220819-1-h3za21_html_fad915d3996ba873.png)

Este comando permite que possamos obter os valores do payload. Agora, vamos criar um método privado para realizar esta leitura. O método é um pouco complexo, portanto separei os trechos em partes para facilitar a explicação:

![](RackMultipart20220819-1-h3za21_html_d1c243ccf42e097e.png)

1 – Verificamos se o tipo conteúdo é um json. Você pode filtrar por outros tipos também como um FormData, mas geralmente, usaremos apenas Json. Se você habilitar para FormData, por exemplo, pode correr o risco de logar um base64 de algum arquivo que você esteja fazendo upload, o que não é legal para nosso log (imagine logar 10mb de texto por causa de um base64)

2 – Criamos um stream para ler o conteúdo do request.Body e definimos que vamos ler o stream desde o inicio.

3 – Recuperamos o valor do payload (body) lendo seu conteúdo até o fim. Na sequência, definimos a posição como 0 (inicio), pois queremos que esta informação seja lida novamente no futuro (é como rebobinar uma fita) e retornamos o valor.

Agora sim, podemos ler e registrar o log:

![](RackMultipart20220819-1-h3za21_html_59d7a0d6a99c027e.png)

Se executarmos, o resultado será:

![](RackMultipart20220819-1-h3za21_html_3a2157beb52950fc.png)

Agora sim temos um log com informações relevantes e de forma automatizada, mas ainda não gravamos em lugar algum. Poderíamos simplesmente instalar um Sink para salvar no banco, mas não queremos deixar o controle com o Serilog.

O primeiro passo para isso, é criando um enricher customizado do Serilog para que consigamos separar os logs do request.

**Serilog Enricher**

Um enricher nada mais é do que uma extensão do serilog. Pegamos o log gerado e fazemos algo com ele, seja uma triagem, tratativa de valores, e claro, a persistência de dados. Em nosso caso, criaremos um enricher responsável por gerar um registro em cache que represente um request e agrupar todos os logs nele.

Primeiramente, crie a classe TraceIdEnricher (falaremos sobre TraceId) e implemente a interface ILogEventEnricher:
 ![](RackMultipart20220819-1-h3za21_html_f25153506a09826c.png)

E nas configurações do Serilog no Program.cs:

![](RackMultipart20220819-1-h3za21_html_ec2f4a2e397e0c49.png)

Ao executarmos, teremos o seguinte resultado:

![](RackMultipart20220819-1-h3za21_html_435886cfbe85412d.png)

Tudo funcionando. Agora, precisamos implementar a real funcionalidade deste enricher.

Cada requisição feita em nossa API recebe uma chave única de identificação, chamada de TraceId. Este traceId será usado como id para nosso registro de log e ele é gerado automaticamente pelo próprio .NET, cabendo a nós apenas o ato de recuperar este valor. E para tal, precisamos injetar IHttpContextAccessor, assim como fizemos em nosso midleware.

Entretanto, os enrichers do serilog não funcionam com injeção de dependência, portanto é necessário fazer alguns workarounds para trabalharmos com a injeção.

![](RackMultipart20220819-1-h3za21_html_cefa04ab0624b924.png)

1 – Crie um construtor vazio e cria uma nova instancia de IHttpContextAcessor. Embora o ideal seja utilizar injeção de dependência, nesse caso a instancia do objeto não importa.

2 – Recupere o traceId. Se tudo correr como esperado, nosso console agora mostra o traceId:

![](RackMultipart20220819-1-h3za21_html_27bcb9e586c9996b.png)

Agora, precisamos salvar os logs em cache. Diferente do HttpContextAcessor, precisamos recuperar a mesma instância do IMemoryCache, caso contrário, perderemos dados. Vamos criar um pequeno serviço que servirá de ponte para as injeções feitas no startup, o ServiceActivator:

![](RackMultipart20220819-1-h3za21_html_7fae6f0f9ad7537e.png)

Como se trata de uma extensão, em nosso Program.cs, simplesmente chamamos o método:

![](RackMultipart20220819-1-h3za21_html_de1bc235ae2b22d0.png)

Dica: O ServiceActivator pode ser útil em várias ocasiões, como por exemplo, acessar um banco de dados dentro de uma classe estática, onde injeção de dependência não funciona.

Agora que temos um meio de recuperar os serviços, precisamos configurar o cache nativo do .NET. Ainda no Program.cs, basta adicionar o MemoryCache aos serviços:

![](RackMultipart20220819-1-h3za21_html_4cd5528bf0a5bf2c.png)

De volta ao TraceIdEnricher, crie um método responsável por instanciar um IMemoryCache utilizando o ServiceActivator:

![](RackMultipart20220819-1-h3za21_html_9cb963b04a3fdf02.png)

Note que verificamos se já existe uma instancia atribuída a \_memoryCache. Isto evita um processamento desnecessário.

O passo seguinte é criar um model que irá representar o agrupamento dos nossos logs. Para tal, crie um "TraceContainer":

![](RackMultipart20220819-1-h3za21_html_d688d0ad2a375f96.png)

Basicamente, criamos um modelo que guarda o traceId junto de uma lista de logs, formando assim um agrupamento.

Com o modelo criado, podemos fazer a recuperação/criação do cache. Nosso método Enrich deve ficar assim:

![](RackMultipart20220819-1-h3za21_html_b669dcb70cc6cdca.png)

Por partes:

1 – Chamamos a configuração do cache que criamos anteriormente. Sem ele, não teremos a referencia do MemoryCache.

2 – Recuperamos uma instancia de TraceContainer utilizando o método GetOrCreate de MemoryCache. Como o nome sugere, ele tenta recuperar um cache a partir de uma chave (no caso, é nosso traceId), e caso não encontre, ele executa a function de "entry".

3 – Definimos um tempo limite de duração do cache. Neste caso é 5 minutos, o que é um tempo bem alto quando se trata de memória do servidor. Muito cuidado ao usar o cache nativo, pois você pode acabar estourando a memória e derrubando sua aplicação. Embora tenhamos definido 5 minutos, iremos excluir esse registro assim que o mesmo não for mais necessário.

4 – Definimos nosso traceId. Você pode colocar informações adicionais, se desejar, mas o básico é apenas o traceId.

5 – Por fim, adicionamos os logs do Serilog na listagem. Perceba que estamos apenas adicionando um item na lista e não dizemos ao cache que ele deve atualizar os dados em nenhum momento.

Para testar, coloque um BreakPoint em algum ponto do método Enrich e execute o projeto. Execute algum endpoint e acompanhe o os logs sendo inseridos na listagem.

![](RackMultipart20220819-1-h3za21_html_f18dcfddc86bb0e5.png)

**Persistir ou descartar logs**

Agora que salvamos nossos logs em cache, podemos voltar em nosso Middleware e decidir o que fazer com o log, como descartar ou salvar no banco de dados.

- Adicione o IMemoryCache na construção do LogMiddleware:
 ![](RackMultipart20220819-1-h3za21_html_1eace850330a1ea3.png)

- No final do método InvokeAsync, adicione um try-catch. Não queremos que um eventual erro na gravação do log invalide a execução do endpoint. Dentro do try, adicione o método que criaremos a seguir, e no catch, alguma tratativa. Neste caso, só vamos mostrar o erro no console.
 ![](RackMultipart20220819-1-h3za21_html_1506cedba108e381.png)

O método ManageLogs ficará da seguinte forma:

![](RackMultipart20220819-1-h3za21_html_cdee6541b6a2aaba.png)

1 – Recuperamos o traceId. Se ele for nulo ou vazio (acontece quando não é um request por API), simplesmente retornamos.

2 – Com o traceId, verificamos se existe no cache e o recuperamos. Se não existir, saímos do método. Na sequência, caso exista, removemos os dados do cache, liberando memória.

3 – Note que o método é async e não usamos await na chamada deste método. O objetivo é não travar o usuário com o pós-processamento do endpoint. Esta é uma técnica de FireAndForget, e a utilizamos para chamar o método SaveTraceContainer criado logo abaixo.

4 – Método async responsável por salvar os dados. Perceba que o resultado do endpoint vai refletir no Swagger, e após os 3 segundos, uma mensagem será exibida no console. Isso demonstra que o usuário foi liberado.

Abaixo um exemplo de como você pode manipular os dados antes de salvar:

![](RackMultipart20220819-1-h3za21_html_6ff25a64f201417e.png)
 ![](RackMultipart20220819-1-h3za21_html_8db2e029bf15645e.png)

E pronto! Temos total controle sobre os logs salvos. Você pode aprimorar cada vez mais os recursos. Você pode adicionar um ExceptionHandler para registrar os logs dos erros, criar regras de exclusão para só registrar o log caso ocorra um erro, ou qualquer outra regra que sua aplicação precisar.

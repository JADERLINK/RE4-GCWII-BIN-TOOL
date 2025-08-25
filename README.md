# RE4-GCWII-BIN-TOOL

Extract and repack BIN files for RE4 OG GC/WII;

**Translate from Portuguese Brazil**

Programa destinado a extrair e reempacotar os arquivos BIN do RE4 OG GC/WII;
<br>Nota1: Além dos BIN do Re4, o programa também extrai os arquivos BIN do RE1 de GC;
<br>Nota2: O repack dos BIN do Re1 são feitos para funcionar no Re4 e não no Re1;
<br>Nota3: VertexColor não são suportados, nem no extract e nem no repack.
<br>Nota4: Morph do RE4 são extraídos, porém, não podem ser feito o repack com eles. Ainda não foi implementada essa funcionalidade no programa.
<br>Nota5: Morph dos Bin de Re1 não são suportados.

## JADERLINK_RE4_GCWII_BIN_TOOL.exe

Programa responsável por extrair e recompilar os arquivos '.bin';
<br>Segue abaixo os "inputs" e "outputs" do programa:

* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.BIN"**
    <br>Extrai o arquivo bin vai gerar os arquivos: 'file.obj', 'file.smd', 'file.mtl', 'file.idxmaterial' e 'file.idxggbin';
* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.OBJ"**
    <br>Faz repack do arquivo '.bin', requisita na mesma pasta o arquivo '.idxggbin' de mesmo nome e o arquivo '.mtl' de mesmo nome;
* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.SMD"**
    <br> Mesma explicação que do arquivo '.obj', so que agora fazendo o repack usando o arquivo '.smd';
* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.MTL"**
    <br>"Extrai" o arquivo '.mtl' cria o arquivo: 'File.Repack.idxmaterial';
* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.IDXMATERIAL"**
    <br> Cria o arquivo 'File.Repack.mtl';
* **JADERLINK_RE4_GCWII_BIN_TOOL.exe "file.IDXGGBIN"**
    <br>Faz repack do arquivo BIN, sem modelo 3D (o modelo 3D fica invisível no jogo) 

## Explicação para que serve cada arquivo:

* .BIN: esse é o modelo 3d do jogo.
* .OBJ: modelo 3d que pode ser editado em um editor 3d;
* .MTL: arquivo que contém os materiais para serem carregados no editor 3d;
* .SMD: (StudioModel Data) modelo 3d que pode ser editado em um editor 3d (com suporte para bones);
* .IDXGGBIN: arquivo necessário para recompilar o arquivo .BIN
* .IDXMATERIAL: é o arquivo que contém os materiais presentes no .bin (pode ser editado);
* .VTA: Quando tem morph, é um arquivo adicional que acompanha o SMD que tem o morph dentro (Atualmente não serve para o repack)
* \_morph\_xx.OBJ: arquivos obj que contem o morph. (Atualmente não serve para o repack)

## Ordem dos bones no arquivo .SMD

Para arrumar a ordem dos ids dos bones nos arquivos smd, depois de serem exportados do blender ou outro software de edição de modelos,<del> usar o programa: GC_GC_Skeleton_Changer.exe (procure o programa no fórum do re4, remod)</del>
<br>Veja: [SMD_BONE_TOOLS](https://github.com/JADERLINK/SMD_BONE_TOOLS)

## Carregando as texturas no arquivo .SMD

No blender para carregar o modelo .SMD com as texturas, em um novo "projeto", importe primeiro o arquivo .obj para ele carregar as texturas, delete o modelo do .obj importado, agora importe o modelo .smd, agora ele será carregado com as texturas.
<br>Lembrando também que as texturas devem estar na pasta com o mesmo nome de seu arquivo .BIN e essa pasta deve estar ao lado do arquivo .mtl;

## Arquivo TPL e extraindo as texturas

As texturas estão dentro do arquivo TPL, para extrair as texturas, use o programa [BrawlCrate](https://github.com/soopercool101/BrawlCrate);
<br>Tutorial com exemplo: vamos supor que você extraiu o arquivo 'pl00_000.BIN', e seu arquivo TPL é o 'pl00_001.TPL';
<br>Primeiro, você tem que renomear o TPL para o mesmo nome do arquivo BIN, então nesse caso o nome vai ficar 'pl00_000.TPL';
<br>Agora abra o TPL no BrawlCrate, selecione todas as texturas (use o Shift do teclado para isso);
<br>Va em 'Edit' > 'Export Selected' (ou Control+E)
<br>Crie uma pasta do mesmo nome do arquivo BIN, no meu caso vai ser 'pl00_000' e selecione-a no programa.
<br>Escolha o formato png e aperte ok.
<br>Se tudo estiver certo, o modelo 3d vai carregar as texturas no seu editor 3d (exemplo Blender)
<br>Nota: você pode usar o BrawlCrate para editar o TPL.

## Código de terceiro:

[ObjLoader by chrisjansson](https://github.com/chrisjansson/ObjLoader):
Encontra-se em "RE4_GCWII_BIN_TOOL\\CjClutter.ObjLoader.Loader", código modificado, as modificações podem ser vistas aqui: [link](https://github.com/JADERLINK/ObjLoader).

**At.te: JADERLINK**
<br>Thanks to "mariokart64n" and "Biohazard4X"
<br>Material information by "Albert"
<br>2025-08-24
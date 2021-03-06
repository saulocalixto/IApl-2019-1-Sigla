﻿using BancoLegal.BancoDeDados.Repositorio;
using BancoLegal.Model;
using BancoLegal.Model.DataAnnotations;
using BancoLegal.Servico.Utilitario;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BancoLegal.Servico
{
    public abstract class ServicoPadrao<T> : IServico<T> where T : ObjetoPadrao
    {
        #region PROPRIEDADES
        protected RepositorioPadrao<T> _repositorio;
        protected UtilitarioDeImportacao _utilitarioDeImportacao;
        protected UtilitarioJson<T> _utilitarioJson;
        protected UtilitarioDeXml<T> _utilitarioXml;
        #endregion

        #region MÉTODOS PÚBLICOS

        protected ServicoPadrao()
        {
            _utilitarioDeImportacao = new UtilitarioDeImportacao();
            _utilitarioJson = new UtilitarioJson<T>();
            _utilitarioXml = new UtilitarioDeXml<T>();
        }

        /// <summary>
        /// Importa um arquivo de texto e salva-o no banco de dados.
        /// </summary>
        /// <param name="caminhoArquivo">Caminho em que se encontra o arquivo.</param>
        public virtual void ImporteArquivo(string caminhoArquivo)
        {
            var linhas = RetorneLinhasDeArquivo(caminhoArquivo);

            var objetos = RetorneObjetos(caminhoArquivo, linhas);

            foreach (var objeto in objetos)
            {
                if(Consulte(objeto.Id).Equals("Conceito não cadastrado."))
                {
                    Repositorio().Cadastre(objeto);
                }
                else
                {
                    Console.Error.WriteLine("Objeto importado de id " + objeto.Id + " já cadastrado.");
                }
            }
        }

        /// <summary>
        /// Consulta um conceito por id e o retorna para o usuário em formato de arquivo de texto.
        /// </summary>
        /// <param name="Id">Id do conceito pesquisado.</param>
        /// <returns>Retorna a string que representa o objeto pesquisado.</returns>
        public string Consulte(int Id)
        {
            T objeto = Repositorio().Consulte(Id);

            if (objeto == null || Id != objeto.Id)
            {
                return "Conceito não cadastrado.";
            }

            var linhaTxt = _utilitarioDeImportacao.TransformeObjetoEmLinha(objeto);
            var linhaJson = _utilitarioJson.ObjetoParaJson(objeto);
            var linhaXml = _utilitarioXml.ObjetoParaString(objeto);

            SalveArquivo(linhaTxt, EnumTipoDeArquivo.TXT);
            SalveArquivo(linhaJson, EnumTipoDeArquivo.JSON);
            SalveArquivo(linhaXml, EnumTipoDeArquivo.XML);

            return linhaTxt;
        }

        public string Consulte()
        {
            var lista = Repositorio().ConsulteTodos();

            if (!lista.Any())
            {
                return "Não há nada cadastrado.";
            }

            var linhaTxt = new StringBuilder();
            var linhaJson = new StringBuilder();
            var linhaXml = new StringBuilder();

            foreach (var objeto in lista)
            {
                linhaTxt.AppendLine(_utilitarioDeImportacao.TransformeObjetoEmLinha(objeto));
                linhaJson.AppendLine(_utilitarioJson.ObjetoParaJson(objeto));
                linhaXml.AppendLine(_utilitarioXml.ObjetoParaString(objeto));
            }

            SalveArquivo(linhaTxt.ToString(), EnumTipoDeArquivo.TXT);
            SalveArquivo(linhaJson.ToString(), EnumTipoDeArquivo.JSON);
            SalveArquivo(linhaXml.ToString(), EnumTipoDeArquivo.XML);

            return linhaTxt.ToString();
        }

        /// <summary>
        /// Atualiza um objeto.
        /// </summary>
        /// <param name="objeto">Objeto a ser atualizado.</param>
        public void Atualize(T objeto)
        {
            if (!Consulte(objeto.Id).Equals("Conceito não cadastrado."))
            {
                Repositorio().Atualize(objeto);
            }
            else
            {
                Console.Error.WriteLine("Objeto importado de id " + objeto.Id + " não foi encontrado.");
            }
        }
        #endregion

        #region MÉTODOS PROTECTED

        protected List<T> RetorneObjetos(string caminhoArquivo, List<string> linhas)
        {
            var lista = new List<T>();

            if (caminhoArquivo.EndsWith("txt"))
            {
                lista = RetorneObjetosDeArquivo(linhas);
            } else if (caminhoArquivo.EndsWith("json"))
            {
                lista = _utilitarioJson.JsonParaObjeto(linhas);
            }
            else
            {
                lista = _utilitarioXml.XmlParaObjetos(linhas);
            }

            return lista;
        }

        protected List<string> RetorneLinhasDeArquivo(string caminho)
        {
            var utilitarioLeitor = new UtilitarioLeitorDeArquivo(caminho);

            var linhas = utilitarioLeitor.RetorneLinhas();

            return linhas;
        }

        protected abstract RepositorioPadrao<T> Repositorio();

        protected abstract string NomeArquivo();
        #endregion

        #region MÉTODOS PRIVADOS
        private List<T> RetorneObjetosDeArquivo(List<string> linhas)
        {
            var objetos = _utilitarioDeImportacao.TransformeLinhaEmObjeto<T>(linhas);
            return objetos;
        }

        private void SalveArquivo(string linha, EnumTipoDeArquivo tipoDeArquivo)
        {
            var extensaoArquivo = string.Empty;

            switch(tipoDeArquivo)
            {
                case EnumTipoDeArquivo.TXT:
                    extensaoArquivo = ".txt";
                    break;
                case EnumTipoDeArquivo.JSON:
                    extensaoArquivo = ".json";
                    break;
                case EnumTipoDeArquivo.XML:
                    extensaoArquivo = ".xml";
                    break;
            }

            string caminho = string.Concat(Environment.CurrentDirectory, @"\" + NomeArquivo() + extensaoArquivo);
            using (StreamWriter file = new StreamWriter(caminho))
            {
                file.Write(linha);
                Console.WriteLine("Arquivo salvo em " + caminho);
            }
        }
        #endregion
    }
}

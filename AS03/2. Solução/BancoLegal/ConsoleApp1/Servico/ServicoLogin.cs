﻿using BancoLegal.BancoDeDados.Repositorio;
using BancoLegal.Model.LoginModel;
using BancoLegal.Servico.Utilitario;

namespace BancoLegal.Servico
{
    public class ServicoLogin
    {
        private UtilitarioLeitorDeArquivo _utilitarioLeitor;
        private UtilitarioDeImportacao _utilitarioDeImportacao;
        private RepositorioContaCorrente _repositorioContaCorrente;

        public ServicoLogin()
        {
            _utilitarioDeImportacao = new UtilitarioDeImportacao();
        }

        public bool EfetueLogin(string caminhoArquivo)
        {
            _utilitarioLeitor = new UtilitarioLeitorDeArquivo(caminhoArquivo);

            var linhas = _utilitarioLeitor.RetorneLinhas();

            var objetos = _utilitarioDeImportacao.TransformeLinhaEmObjeto<Login>(linhas);

            foreach (var objeto in objetos)
            {
                var conta = RepositorioContaCorrente().Consulte(objeto.IdConta);
                return conta.Senha.Equals(objeto.Senha);
            }

            return false;
        }

        private RepositorioContaCorrente RepositorioContaCorrente()
        {
            return _repositorioContaCorrente ?? (_repositorioContaCorrente = new RepositorioContaCorrente());
        }

    }
}

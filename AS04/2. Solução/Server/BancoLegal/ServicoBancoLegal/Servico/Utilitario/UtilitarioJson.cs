﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServicoBancoLegal.Servico.Utilitario
{
    public class UtilitarioJson<T>
    {
        public List<T> JsonParaObjeto(List<string> jsons)
        {
            var lista = new List<T>();

            foreach(var json in jsons)
            {
                lista.Add(JsonConvert.DeserializeObject<T>(json));
            }

            return lista;
        }

        public string ObjetoParaJson(T objeto)
        {
            return JsonConvert.SerializeObject(objeto);
        }
    }
}

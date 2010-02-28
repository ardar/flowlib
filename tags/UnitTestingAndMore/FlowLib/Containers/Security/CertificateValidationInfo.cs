
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

#if !COMPACT_FRAMEWORK
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace FlowLib.Containers.Security
{
    public class CertificateValidationInfo
    {
        protected X509Certificate cert = null;
        protected X509Chain chain = null;
        protected SslPolicyErrors errors = SslPolicyErrors.None;
        protected bool acc = false;

        public X509Certificate Certificate
        {
            get { return cert; }
            set { cert = value; }
        }

        public X509Chain Chain
        {
            get { return chain; }
            set { chain = value; }
        }

        public SslPolicyErrors PolicyErrors
        {
            get { return errors; }
            set { errors = value; }
        }

        public bool Accepted
        {
            get { return acc; }
            set { acc = value; }
        }

        public CertificateValidationInfo(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            this.cert = certificate;
            this.chain = chain;
            this.errors = sslPolicyErrors;
        }
    }
}
#endif

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
using System.Security.Cryptography.X509Certificates;

namespace FlowLib.Containers.Security
{
    public class LocalCertificationSelectionInfo
    {
        protected string target = null;
        protected X509CertificateCollection localCerts = null;
        protected X509Certificate remoteCert = null;
        protected string[] accIssures = null;
        protected X509Certificate selLocalCert = null;

        public string TargetHost
        {
            get { return target; }
            set { target = value; }
        }

        public X509Certificate SelectedCertificate
        {
            get { return selLocalCert; }
            set { selLocalCert = value; }
        }

        public X509CertificateCollection LocalCertifications
        {
            get { return localCerts; }
            set { localCerts = value; }
        }

        public X509Certificate RemoteCertificate
        {
            get { return remoteCert; }
            set { remoteCert = value; }
        }

        public string[] AcceptableIssures
        {
            get { return accIssures; }
            set { accIssures = value; }
        }

        public LocalCertificationSelectionInfo(string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssures)
        {
            this.target = targetHost;
            this.localCerts = localCertificates;
            this.remoteCert = remoteCertificate;
            this.accIssures = acceptableIssures;
        }
    }
}
#endif

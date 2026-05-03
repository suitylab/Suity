Name: suity-agentic
Version: %_version
Release: 1
Summary: Agentic AI Development Environment
License: MIT
URL: https://github.com/ybeapps/suity-agentic
Source: https://github.com/ybeapps/suity-agentic/archive/refs/tags/v%_version.tar.gz
Requires: libX11.so.6()(%{__isa_bits}bit)
Requires: libSM.so.6()(%{__isa_bits}bit)
Requires: libicu
Requires: xdg-utils

%define _build_id_links none

%description
Agentic AI Development Environment

%install
mkdir -p %{buildroot}/opt/suity-agentic
mkdir -p %{buildroot}/%{_bindir}
mkdir -p %{buildroot}/usr/share/applications
mkdir -p %{buildroot}/usr/share/icons
cp -f %{_topdir}/../../Suity.Agentic/* %{buildroot}/opt/suity-agentic/
ln -rsf %{buildroot}/opt/suity-agentic/suity-agentic %{buildroot}/%{_bindir}
cp -r %{_topdir}/../_common/applications %{buildroot}/%{_datadir}
cp -r %{_topdir}/../_common/icons %{buildroot}/%{_datadir}
chmod 755 -R %{buildroot}/opt/suity-agentic
chmod 755 %{buildroot}/%{_datadir}/applications/suity-agentic.desktop

%files
%dir /opt/suity-agentic/
/opt/suity-agentic/*
/usr/share/applications/suity-agentic.desktop
/usr/share/icons/*
%{_bindir}/suity-agentic

%changelog
# skip

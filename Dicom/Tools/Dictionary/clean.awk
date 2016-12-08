BEGIN {
	while(getline < "vr.dat") {
		n = split($0,fields,":");
		types[fields[1]] = $0;
	}
}

{
	if($0 ~ /^\(/ && $NF != "RET" && $(NF-1) in types)
	{
		print $1 ":" $(NF-1) ":" $NF;
	}
}

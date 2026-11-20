(function(root){
 const identity=f=>f.name+':'+f.type+(['checkbox','radio'].includes(f.type)?':'+f.value:'');
 root.IndicaDraftModel={
  key:scope=>'indicaai-draft:v2:'+scope,
  eligible:(name,type)=>!!name&&name!=='__RequestVerificationToken'&&!['hidden','password','submit','file','button','reset'].includes(type),
  changes:(before,after)=>after.filter(field=>{const old=before.find(f=>identity(f)===identity(field));return !old||old.value!==field.value||old.checked!==field.checked;}),
  restorePlan:(saved,version)=>({conflict:String(saved.version)!==String(version),fields:saved.fields}),
  identity
 };
})(typeof window==='undefined'?globalThis:window);

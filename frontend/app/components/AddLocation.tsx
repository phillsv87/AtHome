import React, { useState, useCallback } from 'react';
import { View, StyleSheet, TextInput, Button } from 'react-native';
import { useMounted } from '../CommonJs/Hooks';
import { useApp } from '../lib/hooks';
import Log from '../CommonJs/Log';
import Busy from './Busy';
import { inputStyle } from '../lib/style';

export default function AddLocation()
{

    const {locations,history}=useApp();

    const [name,setName]=useState('');
    const [url,setUrl]=useState('');
    const [token,setToken]=useState('');

    const [busy,setBusy]=useState(false);

    const mt=useMounted();
    const submit=useCallback(async ()=>{

        if(busy){
            return;
        }

        if(!name || !url || !token){
            Log.error('Name, Url and Token required');
            return;
        }

        setBusy(true);
        try{
            await locations.addLocationAsync(name,url,token);
        }catch(ex){
            Log.error('Unable to add location - '+ex.message,ex);
            return;
        }finally{
            setBusy(false);
        }
        if(!mt.mounted){return;}

        history.reset('/');

    },[name,url,token,busy,mt,locations,history]);

    return (
        <View style={styles.root}>
            <TextInput style={inputStyle} placeholderTextColor="#bbb" placeholder="Name" value={name} onChangeText={v=>setName(v)} />
            <TextInput style={inputStyle} placeholderTextColor="#bbb" placeholder="Url" value={url} onChangeText={v=>setUrl(v)} />
            <TextInput style={inputStyle} placeholderTextColor="#bbb" placeholder="Token" value={token} onChangeText={v=>setToken(v)} />

            <Button title="Add" onPress={submit} />

            {busy&&<Busy touchable backgroundColor='#ffffff44'/>}
        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    }
});

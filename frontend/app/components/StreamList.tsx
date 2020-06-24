import React from 'react';
import { useApp } from '../lib/hooks';
import { useAsync } from '../CommonJs/AsyncHooks';
import Log from '../CommonJs/Log';
import { StreamInfo } from '../lib/types';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import Busy from './Busy';

export default function StreamList()
{

    const {http,config,clientToken,history}=useApp();

    const streams=useAsync(null,async ()=>{

        try{

            return await http.getAsync<StreamInfo[]>(`api/Stream`,{clientToken});

        }catch(ex){
            Log.error('Unable to load stream list',ex);
        }

    },[http,config,clientToken]);

    if(!streams){
        return <Busy/>
    }

    return (
        <>
            {streams.map((s)=>(
                <TouchableOpacity key={s.Id} style={styles.item} onPress={()=>history.push('/stream/'+s.Id)}>
                    <View>
                        <Text>{s.Name}</Text>
                    </View>
                </TouchableOpacity>
            ))}
        </>
    )

}


const styles=StyleSheet.create({
    item:{
       padding:10,
       margin:10
    }
});
